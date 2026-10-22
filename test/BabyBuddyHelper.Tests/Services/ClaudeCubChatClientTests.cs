using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Core.Services;
using BabyBuddyHelper.Remote;
using BabyBuddyHelper.Tests.Infrastructure;
using System.Net;
using System.Text.Json;

namespace BabyBuddyHelper.Tests.Services
{
    //The Claude client on a fake network and a dummy key (#96, ADR-018): what it sends, what it returns,
    //and that every failure becomes the one CubUnavailableException (ADR-017).
    public class ClaudeCubChatClientTests
    {
        private const string ParentText = "How long should a 4 month old nap?";

        private static readonly ChatTurn[] OneQuestion = [new(ChatSpeaker.Parent, ParentText)];

        private static ClaudeCubChatClient Client(FakeAnthropicHandler handler, string? key = FakeApiKeyStore.DummyKey, TimeSpan? timeout = null) =>
            new(new FakeApiKeyStore(key), new HttpClient(handler), timeout ?? TimeSpan.FromSeconds(30));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetReplyAsync_NoKey_ThrowsWithNoCall(string? key)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");

            await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler, key).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));

            Assert.Equal(0, handler.Calls);
        }

        [Fact]
        public async Task GetReplyAsync_NoParentTurnToSend_ThrowsWithNoCall()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");

            await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync([new(ChatSpeaker.Cub, "Hello!")], null, TestContext.Current.CancellationToken));

            Assert.Equal(0, handler.Calls);
        }

        [Fact]
        public async Task GetReplyAsync_SendsHaikuTheCapTheRulesAndTheTurns()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");
            ChatTurn[] turns = [new(ChatSpeaker.Parent, "Hi Cub"), new(ChatSpeaker.Cub, "Hello!"), new(ChatSpeaker.Parent, ParentText)];

            await Client(handler).GetReplyAsync(turns, null, TestContext.Current.CancellationToken);

            using JsonDocument body = JsonDocument.Parse(handler.LastBody!);
            JsonElement root = body.RootElement;
            Assert.Equal("claude-haiku-5-5", root.GetProperty("model").GetString());
            Assert.Equal(400, root.GetProperty("max_tokens").GetInt32());
            Assert.Equal(CubGuidelines.BuildSystemPrompt(null), root.GetProperty("system").GetString());
            Assert.Equal(
                [("user", "Hi Cub"), ("assistant", "Hello!"), ("user", ParentText)],
                root.GetProperty("messages").EnumerateArray().Select(message => (message.GetProperty("role").GetString(), message.GetProperty("content").GetString())));
        }

        //The key goes to Anthropic and nowhere else (ADR-010)
        [Fact]
        public async Task GetReplyAsync_SendsTheKeyOnlyToAnthropic()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");

            await Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken);

            Assert.Equal("api.anthropic.com", handler.LastRequest!.RequestUri!.Host);
            Assert.Equal(Uri.UriSchemeHttps, handler.LastRequest.RequestUri.Scheme);
            Assert.Equal($"Bearer {FakeApiKeyStore.DummyKey}", handler.LastRequest.Headers.Authorization?.ToString());
            Assert.DoesNotContain(FakeApiKeyStore.DummyKey, handler.LastBody);
        }

        [Fact]
        public async Task GetReplyAsync_PickedBaby_SendsItsContextInTheRules()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");
            BabyChatContext context = BabyChatContext.From(new BabyModel { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-4) }, [], new VaccineCatalog());

            await Client(handler).GetReplyAsync(OneQuestion, context, TestContext.Current.CancellationToken);

            using JsonDocument body = JsonDocument.Parse(handler.LastBody!);
            Assert.Equal(CubGuidelines.BuildSystemPrompt(context), body.RootElement.GetProperty("system").GetString());
        }

        //At most 20 turns are sent, and the first one sent is the parent's
        [Fact]
        public async Task GetReplyAsync_LongChat_SendsTheTrimmedTurns()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");
            ChatTurn[] turns = Enumerable.Range(1, 25).Select(i => new ChatTurn(i % 2 == 1 ? ChatSpeaker.Parent : ChatSpeaker.Cub, $"turn {i}")).ToArray();

            await Client(handler).GetReplyAsync(turns, null, TestContext.Current.CancellationToken);

            using JsonDocument body = JsonDocument.Parse(handler.LastBody!);
            JsonElement[] sent = body.RootElement.GetProperty("messages").EnumerateArray().ToArray();
            Assert.Equal(19, sent.Length);
            Assert.Equal("user", sent[0].GetProperty("role").GetString());
            Assert.Equal("turn 7", sent[0].GetProperty("content").GetString());
            Assert.Equal("turn 25", sent[^1].GetProperty("content").GetString());
        }

        [Fact]
        public async Task GetReplyAsync_Reply_JoinsItsTextBlocksAndTrims()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("  Naps vary a lot ", "at this age.\n");

            string reply = await Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken);

            Assert.Equal("Naps vary a lot at this age.", reply);
        }

        [Theory]
        [InlineData()]
        [InlineData("   ")]
        public async Task GetReplyAsync_ReplyWithNoText_Throws(params string[] textBlocks)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying(textBlocks);

            await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));
        }

        //401 isn't retried; 429 and 5xx are retried once by the SDK, then fail the same way
        [Theory]
        [InlineData(HttpStatusCode.Unauthorized, 1)]
        [InlineData(HttpStatusCode.BadRequest, 1)]
        [InlineData(HttpStatusCode.TooManyRequests, 2)]
        [InlineData(HttpStatusCode.InternalServerError, 2)]
        [InlineData((HttpStatusCode)529, 2)] //Overloaded
        public async Task GetReplyAsync_ErrorStatus_ThrowsTheOneException(HttpStatusCode status, int expectedCalls)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Failing(status);

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));

            Assert.Equal(expectedCalls, handler.Calls);
            AssertKeepsNoSecret(exception);
        }

        [Fact]
        public async Task GetReplyAsync_NetworkFailure_ThrowsTheOneException()
        {
            var handler = new FakeAnthropicHandler(_ => throw new HttpRequestException("No route to host."));

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));

            AssertKeepsNoSecret(exception);
        }

        //The SDK lets a timeout through as a TaskCanceledException; the caller didn't cancel, so it is a failure
        [Fact]
        public async Task GetReplyAsync_Timeout_ThrowsTheOneException()
        {
            var handler = new FakeAnthropicHandler(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(
                () => Client(handler, timeout: TimeSpan.FromMilliseconds(100)).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));

            AssertKeepsNoSecret(exception);
        }

        //The SDK lets an unreadable reply through as a JsonException
        [Fact]
        public async Task GetReplyAsync_UnreadableReply_ThrowsTheOneException()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Answering(HttpStatusCode.OK, "{not json");

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, null, TestContext.Current.CancellationToken));

            AssertKeepsNoSecret(exception);
        }

        //A page that closes cancels its request; that isn't a failure to show
        [Fact]
        public async Task GetReplyAsync_CallerCancels_PassesTheCancellationThrough()
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            var handler = new FakeAnthropicHandler(async cancellationToken =>
            {
                await cancellation.CancelAsync();
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            Exception exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client(handler).GetReplyAsync(OneQuestion, null, cancellation.Token));

            Assert.IsNotType<CubUnavailableException>(exception);
        }

        //Neither the key nor the parent's message ends up in the exception or anything it wraps (ADR-010)
        private static void AssertKeepsNoSecret(CubUnavailableException exception)
        {
            Assert.DoesNotContain(FakeApiKeyStore.DummyKey, exception.ToString());
            Assert.DoesNotContain(ParentText, exception.ToString());
        }
    }
}
