namespace BabyBuddyHelper.Core.Models
{
    public enum ChatSpeaker
    {
        Parent,
        Cub
    }

    //One message of the chat with Cub. Chats are never stored (#96), so this lives in memory only, for as long as the page is open.
    public sealed record ChatTurn(ChatSpeaker Speaker, string Text);
}
