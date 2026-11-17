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

        /// <summary>
        /// Android only: a modal page (Add task, Add baby, Add vaccine record) opens in a dialog with its own window, so
        /// <see cref="ApplyStatusBar"/> doesn't reach it and its status bar icons stay white on the light bar. This gives
        /// the topmost dialog window the palette's color and dark icons. Called once a modal has been pushed.
        /// </summary>
        public static void ApplyModalStatusBar()
        {
#if ANDROID
            if (!OperatingSystem.IsAndroidVersionAtLeast(23)
                || Application.Current is not { } app
                || !app.Resources.TryGetValue("BabyBuddyBackground", out object? background)
                || background is not Color color
                || Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is not AndroidX.Fragment.App.FragmentActivity activity)
            {
                return;
            }

            //MAUI shows each modal as a DialogFragment; the last one added is the one on top
            AndroidX.Fragment.App.DialogFragment? modal = activity.SupportFragmentManager.Fragments.OfType<AndroidX.Fragment.App.DialogFragment>().LastOrDefault();
            if (modal?.Dialog?.Window is not { } window)
            {
                return;
            }

#pragma warning disable CA1422 //Window color calls are obsolete from API 35, where the bar takes the page's color instead; they still apply below that
            window.SetStatusBarColor(Microsoft.Maui.Platform.ColorExtensions.ToPlatform(color));
#pragma warning restore CA1422
            AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView)?.AppearanceLightStatusBars = true;
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
