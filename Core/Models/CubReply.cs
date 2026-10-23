namespace BabyBuddyHelper.Core.Models
{
    //One reply from the chat provider, before any check (#97). Text is trimmed and may be empty;
    //CutOff is true when the reply stopped at the token cap, so it is unfinished.
    public sealed record CubReply(string Text, bool CutOff);
}
