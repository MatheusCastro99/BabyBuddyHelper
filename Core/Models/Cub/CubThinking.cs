namespace BabyBuddyHelper.Core.Models.Cub
{
    //Whether a call to the chat provider may think before it answers (#155). Thinking counts against the call's token cap,
    //so Cub's replies run with it Off and keep their caps for visible text; the grade thinks a little (Low) under a cap with room for it.
    public enum CubThinking
    {
        Off,
        Low
    }
}
