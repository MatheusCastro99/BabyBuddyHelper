using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Messages;
using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Remote
{
    //Cub's chat replies from Claude, on the parent's own key (#96, ADR-017). It only maps to and from the SDK:
    //the prompt, the cap and the turns come from Core (#97), and the model is fixed. It never writes anything, and it never logs
    //or puts the key or the parent's messages into an exception (ADR-010).
    public sealed class ClaudeCubChatClient : ICubChatClient
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30); //Per attempt; the SDK's own default is 10 minutes
        private const int MaxRetries = 1; //The SDK retries timeouts, 429 and 5xx. One retry keeps the parent's wait near a minute at worst

        private readonly IApiKeyStore _apiKeyStore;
        private readonly HttpClient _httpClient; //One for the app's lifetime; a client per call would leak connections
        private readonly TimeSpan _timeout;

        public ClaudeCubChatClient(IApiKeyStore apiKeyStore) : this(apiKeyStore, new HttpClient(), DefaultTimeout)
        {
        }

        //Tests pass a fake HttpClient and a short timeout, so no test reaches the network (ADR-018)
        internal ClaudeCubChatClient(IApiKeyStore apiKeyStore, HttpClient httpClient, TimeSpan timeout)
        {
            _apiKeyStore = apiKeyStore;
            _httpClient = httpClient;
            _timeout = timeout;
        }

        public async Task<CubReply> GetReplyAsync(IReadOnlyList<ChatTurn> turns, string systemPrompt, int maxTokens, CancellationToken cancellationToken = default)
        {
            string? apiKey = await _apiKeyStore.GetKeyAsync();

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new CubUnavailableException("No Claude key is saved.");

            MessageCreateParams request = BuildRequest(turns, systemPrompt, maxTokens);

            if (request.Messages.Count == 0)
                throw new CubUnavailableException("There is no message from the parent to send.");

            try
            {
                var client = new AnthropicClient(new ClientOptions
                {
                    ApiKey = apiKey, //Setting it also stops the SDK reading a key from the environment
                    HttpClient = _httpClient,
                    Timeout = _timeout,
                    MaxRetries = MaxRetries
                });

                Message reply = await client.Messages.Create(request, cancellationToken);

                //Inside the try: the SDK checks a reply's shape only when it is read, so a wrong one throws here
                return MapReply(reply);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) //The caller gave up; not a failure
            {
                throw;
            }
            catch (Exception exception) //API errors, network errors, the SDK's timeout and unreadable replies all mean Cub can't answer
            {
                throw new CubUnavailableException("Claude couldn't answer.", exception);
            }
        }

        internal static MessageCreateParams BuildRequest(IReadOnlyList<ChatTurn> turns, string systemPrompt, int maxTokens)
        {
            List<MessageParam> messages = turns
                .Select(turn => turn.Speaker == ChatSpeaker.Parent
                    ? (MessageParam)new UserMessageParam { Content = turn.Text }
                    : new AssistantMessageParam { Content = turn.Text })
                .ToList();

            return new MessageCreateParams
            {
                Model = Model.ClaudeHaiku5_5,
                MaxTokens = maxTokens,
                System = systemPrompt,
                Messages = messages
            };
        }

        //The reply is the text blocks, in order. Core's checks decide what an empty or cut-off reply means (#97)
        internal static CubReply MapReply(Message reply)
        {
            string text = string.Concat(reply.Content.Select(block => block.TryPickText(out TextBlock? textBlock) ? textBlock.Text : string.Empty)).Trim();

            return new CubReply(text, reply.StopReason == StopReason.MaxTokens);
        }
    }
}
