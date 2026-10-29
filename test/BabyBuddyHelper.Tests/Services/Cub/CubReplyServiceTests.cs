using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Services.Cub;
using BabyBuddyHelper.Core.Services.Vaccine;
using BabyBuddyHelper.Tests.Infrastructure;
using static BabyBuddyHelper.Tests.Infrastructure.FakeCubChatClient;

namespace BabyBuddyHelper.Tests.Services.Cub
{
    //The sanity layer on a scripted chat client (#97, ADR-020): what is shown, what is retried with which note,
    //and that a grader that can't answer fails closed. Whether Claude grades well is the manual checklist's job.
    public class CubReplyServiceTests
    {
        private const string ParentText = "How long should a 4 month old nap?";
        private const string GoodReply = "Naps vary a lot at this age. Watch for sleepy cues.";
        private const string GraderReason = "FAIL: the reply sounds cold and skips the pediatrician nudge";

        private static readonly ChatTurn[] OneQuestion = [new(ChatSpeaker.Parent, ParentText)];
        private static readonly CubPersonality Personality = new(CubStyle.Direct, CubVerbosity.Short);

        private static Task<CubOutcome> Ask(FakeCubChatClient client, IReadOnlyList<ChatTurn>? history = null, CancellationToken? cancellationToken = null) =>
            new CubReplyService(client).AskAsync(history ?? OneQuestion, null, Personality, cancellationToken ?? TestContext.Current.CancellationToken);

        [Fact]
        public async Task AskAsync_GoodReplyAndPass_ShowsItAfterTwoCalls()
        {
            var client = new FakeCubChatClient(Says(GoodReply), Says("PASS"));

            CubOutcome outcome = await Ask(client);

            Assert.Equal(new CubOutcome.Reply(GoodReply), outcome);
            Assert.Equal(2, client.Requests.Count);
        }

        //Cub is asked with the parent's style, length and cap; the grader with its own prompt, the reply and its small cap
        [Fact]
        public async Task AskAsync_SendsTheReplyRequestThenTheGradeRequest()
        {
            var client = new FakeCubChatClient(Says(GoodReply), Says("PASS"));

            await Ask(client);

            Request ask = client.Requests[0];
            Assert.Equal(CubGuidelines.BuildSystemPrompt(null, Personality), ask.SystemPrompt);
            Assert.Equal(250, ask.MaxTokens);
            Assert.Equal(OneQuestion, ask.Turns);

            Request grade = client.Requests[1];
            Assert.Equal(CubGuidelines.BuildGraderPrompt(Personality, babyPicked: false), grade.SystemPrompt);
            Assert.Equal(CubGuidelines.MaxGradeTokens, grade.MaxTokens);
            Assert.Equal([new ChatTurn(ChatSpeaker.Parent, CubGuidelines.BuildGradeRequest(ParentText, GoodReply))], grade.Turns);
        }

        //At most 20 turns are sent, and the first one sent is the parent's
        [Fact]
        public async Task AskAsync_LongChat_SendsTheTrimmedTurns()
        {
            ChatTurn[] history = Enumerable.Range(1, 25).Select(i => new ChatTurn(i % 2 == 1 ? ChatSpeaker.Parent : ChatSpeaker.Cub, $"turn {i}")).ToArray();
            var client = new FakeCubChatClient(Says(GoodReply), Says("PASS"));

            await Ask(client, history);

            IReadOnlyList<ChatTurn> sent = client.Requests[0].Turns;
            Assert.Equal(19, sent.Count);
            Assert.Equal(new ChatTurn(ChatSpeaker.Parent, "turn 7"), sent[0]);
            Assert.Equal(new ChatTurn(ChatSpeaker.Parent, "turn 25"), sent[^1]);
            Assert.Contains("turn 25", client.Requests[1].Turns[0].Text); //The grader checks against the newest message
        }

        //The grader's prompt holds every rule, the chosen style and length, and the language check
        [Fact]
        public void BuildGraderPrompt_HoldsTheRulesStyleAndLength()
        {
            string prompt = CubGuidelines.BuildGraderPrompt(Personality, babyPicked: false);

            Assert.Contains("Never be negative", prompt);
            Assert.Contains("contact emergency services now", prompt);
            Assert.Contains("Decline off-topic requests graciously", prompt);
            Assert.Contains("In an emergency, the health and safety rules come before the style and length.", prompt);
            Assert.Contains(CubGuidelines.StyleLine(CubStyle.Direct), prompt);
            Assert.Contains(CubGuidelines.LengthLine(CubVerbosity.Short), prompt);
            Assert.Contains("same language as the parent's message", prompt);
            Assert.Contains("instructions above that apply to this message", prompt);
            Assert.Contains("Fail it only for a clear break", prompt);
            Assert.Contains("answer with the single word PASS", prompt);
        }

        //With "General advice", the grader holds that rule too, so a made-up name fails
        [Fact]
        public void BuildGraderPrompt_GeneralAdvice_HoldsItsRule()
        {
            string prompt = CubGuidelines.BuildGraderPrompt(Personality, babyPicked: false);

            Assert.Contains("\"General advice\"", prompt);
            Assert.Contains("don't assume the baby's name, age or details", prompt);
            Assert.DoesNotContain("using them is fine", prompt);
            Assert.DoesNotContain("You can't see those details", prompt);
        }

        //With a picked baby, the grader knows Cub was told the details, but never gets them
        [Fact]
        public void BuildGraderPrompt_PickedBaby_AllowsItsDetails()
        {
            string prompt = CubGuidelines.BuildGraderPrompt(Personality, babyPicked: true);

            Assert.Contains("The app told Cub the baby's name and details, so using them is fine.", prompt);
            Assert.Contains("You can't see those details", prompt);
            Assert.DoesNotContain("\"General advice\"", prompt);
            Assert.DoesNotContain("The parent picked", prompt);
        }

        [Fact]
        public async Task AskAsync_PickedBaby_GradesWithThePickedBabyPrompt()
        {
            BabyChatContext baby = BabyChatContext.From(new BabyModel { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-4) }, [], new VaccineCatalog());
            var client = new FakeCubChatClient(Says(GoodReply), Says("PASS"));

            await new CubReplyService(client).AskAsync(OneQuestion, baby, Personality, TestContext.Current.CancellationToken);

            Assert.Equal(CubGuidelines.BuildGraderPrompt(Personality, babyPicked: true), client.Requests[1].SystemPrompt);
        }

        //Code fails never reach the grader; the retry carries the fixed note for the reason, and a good retry is shown
        [Theory]
        [InlineData("cut off", "Your last answer was too long and got cut off. Answer again, more briefly.")]
        [InlineData("markdown", "Your last answer used formatting. Answer again in plain text only.")]
        public async Task AskAsync_CodeCheckFails_RetriesWithItsNote(string failure, string note)
        {
            Func<CubReply> bad = failure == "cut off" ? SaysCutOff("Naps vary a lot at") : Says("Naps **vary** a lot.");
            var client = new FakeCubChatClient(bad, Says(GoodReply), Says("PASS"));

            CubOutcome outcome = await Ask(client);

            Assert.Equal(new CubOutcome.Reply(GoodReply), outcome);
            Assert.Equal(3, client.Requests.Count); //Reply, retry, grade: the bad reply was never graded
            Assert.Equal($"{client.Requests[0].SystemPrompt}{Environment.NewLine}{Environment.NewLine}{note}", client.Requests[1].SystemPrompt);
            Assert.Equal(client.Requests[0].Turns, client.Requests[1].Turns); //The failed reply is never sent back
            Assert.Equal(client.Requests[0].MaxTokens, client.Requests[1].MaxTokens);
        }

        //An empty reply has no note: the same request is simply asked again
        [Fact]
        public async Task AskAsync_EmptyReply_AsksAgainUnchanged()
        {
            var client = new FakeCubChatClient(Says(string.Empty), Says(GoodReply), Says("PASS"));

            CubOutcome outcome = await Ask(client);

            Assert.Equal(new CubOutcome.Reply(GoodReply), outcome);
            Assert.Equal(client.Requests[0], client.Requests[1]);
        }

        [Fact]
        public async Task AskAsync_GraderFails_RetriesWithItsNoteAndGradesAgain()
        {
            var client = new FakeCubChatClient(Says("Whatever."), Says(GraderReason), Says(GoodReply), Says("PASS"));

            CubOutcome outcome = await Ask(client);

            Assert.Equal(new CubOutcome.Reply(GoodReply), outcome);
            Assert.Equal(4, client.Requests.Count);
            Assert.EndsWith("Your last answer did not follow your rules or style. Answer again, following them carefully.", client.Requests[2].SystemPrompt);
            Assert.Equal(CubGuidelines.BuildGradeRequest(ParentText, GoodReply), client.Requests[3].Turns[0].Text);
        }

        //The grader's words are model text: they are never shown and never sent back to Cub
        [Fact]
        public async Task AskAsync_GraderFails_ItsReasonNeverReachesTheRetry()
        {
            var client = new FakeCubChatClient(Says("Whatever."), Says(GraderReason), Says(GoodReply), Says("PASS"));

            await Ask(client);

            Request retry = client.Requests[2];
            Assert.DoesNotContain("pediatrician nudge", retry.SystemPrompt);
            Assert.DoesNotContain(retry.Turns, turn => turn.Text.Contains("pediatrician nudge"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("PASSED")]
        [InlineData("I'm not sure.")]
        public async Task AskAsync_UnreadableGrade_CountsAsAFail(string grade)
        {
            var client = new FakeCubChatClient(Says(GoodReply), Says(grade), Says(GoodReply), Says(grade));

            Assert.Equal(new CubOutcome.Declined(), await Ask(client));
            Assert.Equal(4, client.Requests.Count);
        }

        //A grade that wraps PASS in formatting still passes (#155)
        [Fact]
        public async Task AskAsync_FormattedPassGrade_ShowsTheReply()
        {
            var client = new FakeCubChatClient(Says(GoodReply), Says("**PASS**"));

            Assert.Equal(new CubOutcome.Reply(GoodReply), await Ask(client));
            Assert.Equal(2, client.Requests.Count);
        }

        //Exactly one retry: whatever fails twice is declined, and nothing more is asked
        [Fact]
        public async Task AskAsync_TwoFails_IsDeclined()
        {
            var client = new FakeCubChatClient(SaysCutOff("Naps vary"), Says("Naps **vary**."));

            Assert.Equal(new CubOutcome.Declined(), await Ask(client));
            Assert.Equal(2, client.Requests.Count);
        }

        [Fact]
        public async Task AskAsync_RetryFailsTheGrader_IsDeclined()
        {
            var client = new FakeCubChatClient(Says("Naps **vary**."), Says("Whatever."), Says(GraderReason));

            Assert.Equal(new CubOutcome.Declined(), await Ask(client));
            Assert.Equal(3, client.Requests.Count);
        }

        //Fail closed: a grader that can't answer shows the "couldn't answer just now" line, never the ungraded reply
        [Fact]
        public async Task AskAsync_GraderThrows_Propagates()
        {
            var client = new FakeCubChatClient(Says(GoodReply), Throws(new CubUnavailableException("Claude couldn't answer.")));

            await Assert.ThrowsAsync<CubUnavailableException>(() => Ask(client));
        }

        [Fact]
        public async Task AskAsync_GraderThrowsOnTheRetry_Propagates()
        {
            var client = new FakeCubChatClient(Says("Whatever."), Says(GraderReason), Says(GoodReply), Throws(new CubUnavailableException("Claude couldn't answer.")));

            await Assert.ThrowsAsync<CubUnavailableException>(() => Ask(client));
        }

        [Fact]
        public async Task AskAsync_ReplyThrows_Propagates()
        {
            var client = new FakeCubChatClient(Throws(new CubUnavailableException("No Claude key is saved.")));

            await Assert.ThrowsAsync<CubUnavailableException>(() => Ask(client));
        }

        [Fact]
        public async Task AskAsync_CallerCancels_PassesTheCancellationThrough()
        {
            using var cancellation = new CancellationTokenSource();
            var client = new FakeCubChatClient(() => { cancellation.Cancel(); return new CubReply(GoodReply, CutOff: false); }, Says("PASS"));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Ask(client, cancellationToken: cancellation.Token));
            Assert.Single(client.Requests); //The grader is never asked once the caller gave up
        }
    }
}
