namespace BabyBuddyHelper.Core.Models
{
    //How Cub talks in the chat (#97). The names are saved as they are, so renaming one resets it to the default.
    public enum CubStyle
    {
        Supportive,
        Neutral,
        Direct
    }

    public enum CubVerbosity
    {
        Short,
        Balanced,
        Detailed
    }

    //The parent's choice for Cub's chat replies. It shapes the replies only: tips, toasts and animations don't change.
    public sealed record CubPersonality(CubStyle Style, CubVerbosity Verbosity)
    {
        public static CubPersonality Default { get; } = new(CubStyle.Supportive, CubVerbosity.Balanced);
    }
}
