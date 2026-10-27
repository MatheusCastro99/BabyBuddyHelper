namespace BabyBuddyHelper.Core.Models.Cub
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

        //Reads the names a store saved. A missing or unknown name (first run, or one from another version) falls back to
        //that half of the default, as the palette does (PaletteService)
        public static CubPersonality FromSaved(string? style, string? verbosity) => new(
            Enum.TryParse(style, out CubStyle savedStyle) && Enum.IsDefined(savedStyle) ? savedStyle : Default.Style,
            Enum.TryParse(verbosity, out CubVerbosity savedVerbosity) && Enum.IsDefined(savedVerbosity) ? savedVerbosity : Default.Verbosity);
    }
}
