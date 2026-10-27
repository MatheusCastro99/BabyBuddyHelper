using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Services.Cub;
using BabyBuddyHelper.Remote;
using BabyBuddyHelper.Tests.Infrastructure;
using System.Net;
using System.Text.Json;

namespace BabyBuddyHelper.Tests.Services
{
    //The Claude client on a fake network and a dummy key (#96, ADR-018): what it sends, what it returns,
    //and that every failure becomes the one CubUnavailableException (ADR-017). Core builds the prompt and the cap (#97),
    //so the client is given stand-ins and only has to send them as they are.
    public class ClaudeCubChatClientTests
    {
        private const string ParentText = "How long should a 4 month old nap?";
        private const string Rules = "Test rules.";
        private const int Cap = 400;

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

            await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler, key).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

            Assert.Equal(0, handler.Calls);
        }

        [Fact]
        public async Task GetReplyAsync_NoTurnToSend_ThrowsWithNoCall()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");

            await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync([], Rules, Cap, TestContext.Current.CancellationToken));

            Assert.Equal(0, handler.Calls);
        }

        //Each reply length sends its own cap; the turns go as given, since Core trims them
        [Theory]
        [InlineData(250)]
        [InlineData(400)]
        [InlineData(700)]
        public async Task GetReplyAsync_SendsHaikuTheGivenCapRulesAndTurns(int cap)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");
            ChatTurn[] turns = [new(ChatSpeaker.Parent, "Hi Cub"), new(ChatSpeaker.Cub, "Hello!"), new(ChatSpeaker.Parent, ParentText)];
            string rules = CubGuidelines.BuildSystemPrompt(null, CubPersonality.Default);

            await Client(handler).GetReplyAsync(turns, rules, cap, TestContext.Current.CancellationToken);

            using JsonDocument body = JsonDocument.Parse(handler.LastBody!);
            JsonElement root = body.RootElement;
            Assert.Equal("claude-haiku-5-5", root.GetProperty("model").GetString());
            Assert.Equal(cap, root.GetProperty("max_tokens").GetInt32());
            Assert.Equal(rules, root.GetProperty("system").GetString());
            Assert.Equal(
                [("user", "Hi Cub"), ("assistant", "Hello!"), ("user", ParentText)],
                root.GetProperty("messages").EnumerateArray().Select(message => (message.GetProperty("role").GetString(), message.GetProperty("content").GetString())));
        }

        //The key goes to Anthropic and nowhere else (ADR-010)
        [Fact]
        public async Task GetReplyAsync_SendsTheKeyOnlyToAnthropic()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("Hi!");

            await Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken);

            Assert.Equal("api.anthropic.com", handler.LastRequest!.RequestUri!.Host);
            Assert.Equal(Uri.UriSchemeHttps, handler.LastRequest.RequestUri.Scheme);
            Assert.Equal($"Bearer {FakeApiKeyStore.DummyKey}", handler.LastRequest.Headers.Authorization?.ToString());
            Assert.DoesNotContain(FakeApiKeyStore.DummyKey, handler.LastBody);
        }

        [Fact]
        public async Task GetReplyAsync_Reply_JoinsItsTextBlocksAndTrims()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying("  Naps vary a lot ", "at this age.\n");

            CubReply reply = await Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken);

            Assert.Equal(new CubReply("Naps vary a lot at this age.", CutOff: false), reply);
        }

        //Core's checks decide what an empty reply means (#97), so it comes back instead of throwing
        [Theory]
        [InlineData()]
        [InlineData("   ")]
        public async Task GetReplyAsync_ReplyWithNoText_ReturnsItEmpty(params string[] textBlocks)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Replying(textBlocks);

            CubReply reply = await Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken);

            Assert.Equal(new CubReply(string.Empty, CutOff: false), reply);
        }

        //Only a reply stopped by the token cap is cut off
        [Theory]
        [InlineData("max_tokens", true)]
        [InlineData("end_turn", false)]
        [InlineData("stop_sequence", false)]
        [InlineData("refusal", false)]
        public async Task GetReplyAsync_StopReason_SetsCutOff(string stopReason, bool cutOff)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Stopping(stopReason, "Naps vary a lot at");

            CubReply reply = await Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken);

            Assert.Equal(new CubReply("Naps vary a lot at", cutOff), reply);
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

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

            Assert.Equal(expectedCalls, handler.Calls);
            AssertKeepsNoSecret(exception);
        }

        [Fact]
        public async Task GetReplyAsync_NetworkFailure_ThrowsTheOneException()
        {
            var handler = new FakeAnthropicHandler(_ => throw new HttpRequestException("No route to host."));

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

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
                () => Client(handler, timeout: TimeSpan.FromMilliseconds(100)).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

            AssertKeepsNoSecret(exception);
        }

        //The SDK lets an unreadable reply through as a JsonException
        [Fact]
        public async Task GetReplyAsync_UnreadableReply_ThrowsTheOneException()
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Answering(HttpStatusCode.OK, "{not json");

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

            AssertKeepsNoSecret(exception);
        }

        //Valid JSON that isn't a message passes the SDK's call and only fails when the reply is read (its validation is lazy)
        [Theory]
        [InlineData("{}")]
        [InlineData("""{"content":"x"}""")]
        public async Task GetReplyAsync_ReplyOfTheWrongShape_ThrowsTheOneException(string body)
        {
            FakeAnthropicHandler handler = FakeAnthropicHandler.Answering(HttpStatusCode.OK, body);

            CubUnavailableException exception = await Assert.ThrowsAsync<CubUnavailableException>(() => Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, TestContext.Current.CancellationToken));

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

            Exception exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client(handler).GetReplyAsync(OneQuestion, Rules, Cap, cancellation.Token));

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
