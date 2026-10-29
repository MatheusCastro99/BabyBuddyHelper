using BabyBuddyHelper.Core.Models.Cub;

namespace BabyBuddyHelper.Core.Interfaces.Cub
{
    public interface ICubChatClient //The off-device path for Cub's chat replies (ADR-017). Core talks to this; the implementation lives in Remote/
    {
        //Sends the turns as given, under the given system prompt, output-token cap and thinking, and returns the whole reply unchecked.
        //Core builds the prompt, the cap, the thinking and the turns (#97, #155), so the same call serves Cub's reply and its grade.
        //Throws only CubUnavailableException, or OperationCanceledException when the caller cancels.
        Task<CubReply> GetReplyAsync(IReadOnlyList<ChatTurn> turns, string systemPrompt, int maxTokens, CubThinking thinking,
            CancellationToken cancellationToken = default);
    }
}
