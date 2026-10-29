using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Models.Cub;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //A chat client that answers from a script, one step per call, and records each request (#97). It stands in for Claude
    //under CubReplyService, so the sanity layer is tested without the SDK or the network.
    public sealed class FakeCubChatClient(params Func<CubReply>[] script) : ICubChatClient
    {
        public sealed record Request(IReadOnlyList<ChatTurn> Turns, string SystemPrompt, int MaxTokens, CubThinking Thinking);

        private readonly Queue<Func<CubReply>> _script = new(script);

        public List<Request> Requests { get; } = [];

        //Script steps: a reply, a reply cut off at the cap, or a call that throws
        public static Func<CubReply> Says(string text) => () => new CubReply(text, CutOff: false);
        public static Func<CubReply> SaysCutOff(string text) => () => new CubReply(text, CutOff: true);
        public static Func<CubReply> Throws(Exception exception) => () => throw exception;

        public Task<CubReply> GetReplyAsync(IReadOnlyList<ChatTurn> turns, string systemPrompt, int maxTokens, CubThinking thinking,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(new Request(turns, systemPrompt, maxTokens, thinking));

            if (_script.Count == 0)
                throw new InvalidOperationException($"The script has no step for call {Requests.Count}.");

            return Task.FromResult(_script.Dequeue()());
        }
    }
}
