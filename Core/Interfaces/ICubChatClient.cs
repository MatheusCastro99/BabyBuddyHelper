using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Core.Interfaces
{
    public interface ICubChatClient //The off-device path for Cub's chat replies (ADR-017). Pages talk to this; the implementation lives in Remote/
    {
        //Sends the chat so far, and the picked baby's context when there is one (null for "General advice"), and returns Cub's whole reply.
        //Throws only CubUnavailableException, or OperationCanceledException when the caller cancels.
        Task<string> GetReplyAsync(IReadOnlyList<ChatTurn> turns, BabyChatContext? babyContext, CancellationToken cancellationToken = default);
    }
}
