using BabyBuddyHelper.Resources.Styles.Palettes;

namespace BabyBuddyHelper.UI.Services
{
    /// <summary>
    /// The app's two color palettes. They take the place of a light / dark mode.
    /// </summary>
    public enum PaletteStyle
    {
        Boy,
        Girl,
    }

    /// <summary>
    /// Applies the boy or girl palette to the whole app and remembers the choice between launches.
    /// The palette is a device UI setting, not domain data, so it lives in Preferences and never touches the database.
    /// </summary>
    public static class PaletteService
    {
        const string PreferenceKey = "palette_style";

        public static PaletteStyle Current { get; private set; } = PaletteStyle.Boy;

        /// <summary>
        /// Applies the palette saved on this device. Called once at startup, before any page exists.
        /// </summary>
        public static void ApplySaved()
        {
            //A missing or unreadable value (first run, or a name from another version) falls back to Boy
            string saved = Preferences.Default.Get(PreferenceKey, nameof(PaletteStyle.Boy));
            Swap(Enum.TryParse(saved, out PaletteStyle style) && Enum.IsDefined(style) ? style : PaletteStyle.Boy);
        }

        public static void Apply(PaletteStyle style)
        {
            if (style == Current)
            {
                return;
            }

            Swap(style);
            Preferences.Default.Set(PreferenceKey, style.ToString());
            ApplyStatusBar();
        }

        /// <summary>
        /// Android only: paints the status bar in the palette's background color, with dark icons, so it reads as part of
        /// the nav band under it. It needs a window, so startup calls it from the window's Created event and a palette
        /// switch calls it after the swap. Other platforms have nothing to paint.
        /// </summary>
        public static void ApplyStatusBar()
        {
#if ANDROID
            //Dark status bar icons need Android 6 (API 23); the toolkit's calls are marked the same
            if (OperatingSystem.IsAndroidVersionAtLeast(23) && Application.Current is { } app && app.Resources.TryGetValue("BabyBuddyBackground", out object? background) && background is Color color)
            {
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(color);
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(CommunityToolkit.Maui.Core.StatusBarStyle.DarkContent);
            }
#endif
        }

        //Adds the new palette before removing the old one, so no color key is ever missing while pages are on screen.
        //Colors.xaml and Styles.xaml stay merged; only the palette dictionary changes.
        static void Swap(PaletteStyle style)
        {
            if (Application.Current is not { } app)
            {
                return;
            }

            ICollection<ResourceDictionary> merged = app.Resources.MergedDictionaries;
            List<ResourceDictionary> old = [.. merged.Where(d => d is BoyPalette or GirlPalette)];

            merged.Add(style == PaletteStyle.Girl ? new GirlPalette() : new BoyPalette());
            foreach (ResourceDictionary palette in old)
            {
                merged.Remove(palette);
            }

            Current = style;
        }
    }
}
