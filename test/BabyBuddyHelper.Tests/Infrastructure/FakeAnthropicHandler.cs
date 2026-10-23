using System.Net;
using System.Text;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //Stands in for the network under the Anthropic SDK, so no test reaches Claude or needs a real key (ADR-010, ADR-018).
    //It records each request and answers every call with the same response, or with what the given function does.
    public sealed class FakeAnthropicHandler : HttpMessageHandler
    {
        private readonly Func<CancellationToken, Task<HttpResponseMessage>> _respond;

        public int Calls { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastBody { get; private set; }

        public FakeAnthropicHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond)
        {
            _respond = respond;
        }

        //A whole reply made of the given text blocks
        public static FakeAnthropicHandler Replying(params string[] textBlocks) => Stopping("end_turn", textBlocks);

        //A reply made of the given text blocks that stopped for the given reason, such as "max_tokens" for one cut off at the cap
        public static FakeAnthropicHandler Stopping(string stopReason, params string[] textBlocks)
        {
            string content = string.Join(",", textBlocks.Select(text => $$"""{"type":"text","text":{{System.Text.Json.JsonSerializer.Serialize(text)}}}"""));

            return Answering(HttpStatusCode.OK,
                """{"id":"msg_test","type":"message","role":"assistant","model":"claude-haiku-5-5","content":[""" + content
                + $$$"""],"stop_reason":{{{System.Text.Json.JsonSerializer.Serialize(stopReason)}}},"stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":1}}""");
        }

        public static FakeAnthropicHandler Failing(HttpStatusCode status) =>
            Answering(status, """{"type":"error","error":{"type":"test_error","message":"Test failure."}}""");

        public static FakeAnthropicHandler Answering(HttpStatusCode status, string body) =>
            new(_ => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") }));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            return await _respond(cancellationToken);
        }
    }
}
