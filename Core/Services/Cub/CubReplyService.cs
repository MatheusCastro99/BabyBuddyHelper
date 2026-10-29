using System.Diagnostics;
using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Models.Cub;

namespace BabyBuddyHelper.Core.Services.Cub
{
    //The sanity layer between Claude and the parent (#97, ADR-020). A reply is shown only after it passes the code checks
    //and the grader answers PASS; a reply that fails gets exactly one retry, with a fixed note for the reason.
    //CubUnavailableException and cancellation pass through untouched, so a grader that can't answer fails closed.
    public sealed class CubReplyService(ICubChatClient chatClient)
    {
        //history ends with the parent's new message; the outcome is never added to it here
        public async Task<CubOutcome> AskAsync(IReadOnlyList<ChatTurn> history, BabyChatContext? babyContext, CubPersonality personality,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ChatTurn> turns = CubGuidelines.TrimHistory(history);
            string systemPrompt = CubGuidelines.BuildSystemPrompt(babyContext, personality);
            int maxTokens = CubGuidelines.MaxReplyTokensFor(personality.Verbosity);
            string parentMessage = turns.LastOrDefault(turn => turn.Speaker == ChatSpeaker.Parent)?.Text ?? string.Empty;

            CubReply reply = await chatClient.GetReplyAsync(turns, systemPrompt, maxTokens, CubThinking.Off, cancellationToken);
            CubReplyProblem? problem = await FindProblemAsync(reply, parentMessage, personality, babyContext is not null, cancellationToken);

            if (problem is null)
                return new CubOutcome.Reply(reply.Text);

            //The retry is the same request with the fixed note added to the rules; the failed reply is never sent back
            string? note = CubGuidelines.RetryNote(problem.Value);
            string retryPrompt = note is null ? systemPrompt : $"{systemPrompt}{Environment.NewLine}{Environment.NewLine}{note}";

            CubReply retry = await chatClient.GetReplyAsync(turns, retryPrompt, maxTokens, CubThinking.Off, cancellationToken);

            return await FindProblemAsync(retry, parentMessage, personality, babyContext is not null, cancellationToken) is null
                ? new CubOutcome.Reply(retry.Text)
                : new CubOutcome.Declined();
        }

        //The code checks first, then the grader, which is only paid for when the code finds nothing
        private async Task<CubReplyProblem?> FindProblemAsync(CubReply reply, string parentMessage, CubPersonality personality, bool babyPicked,
            CancellationToken cancellationToken)
        {
            CubReplyProblem? problem = CubReplyChecks.Check(reply);

            if (problem is not null)
            {
                TraceFailure($"the code checks: {problem}");
                return problem;
            }

            CubReply grade = await chatClient.GetReplyAsync(
                [new ChatTurn(ChatSpeaker.Parent, CubGuidelines.BuildGradeRequest(parentMessage, reply.Text))],
                CubGuidelines.BuildGraderPrompt(personality, babyPicked), CubGuidelines.MaxGradeTokens, CubThinking.Low, cancellationToken);

            if (CubReplyChecks.IsPassingGrade(grade.Text))
                return null;

            //A grade with no text says whether its cap ran out, so an empty line in the trace explains itself (#155)
            string why = grade.Text.Length > 0 ? grade.Text : grade.CutOff ? "(cut off with no text)" : "(no text)";
            TraceFailure($"the grade: {why}");
            return CubReplyProblem.FailedGrade;
        }

        //Debug-only (#155): says why a try failed, so a declined row in the checklist run explains itself. Debug.WriteLine is
        //[Conditional("DEBUG")], so Release builds drop the call. Never the parent's message, Cub's reply, the prompt or the key (ADR-010).
        private static void TraceFailure(string why) => Debug.WriteLine($"Cub reply failed {why}");
    }
}
