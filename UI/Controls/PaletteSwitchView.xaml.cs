using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Switches the app between the boy and girl palettes (off = boy, on = girl). It sits on the Shell tab row,
    /// at the far right, so it is reachable from every tab without living on any page.
    /// </summary>
    public partial class PaletteSwitchView : ContentView
    {
        public PaletteSwitchView()
        {
            InitializeComponent();

            //Shows the saved palette before listening, so restoring it doesn't count as a user switch
            PaletteToggle.IsToggled = PaletteService.Current == PaletteStyle.Girl;
            UpdateDescription();
            PaletteToggle.Toggled += OnPaletteToggled;

            PaletteToggle.Focused += (_, _) => FocusRing.IsVisible = HasKeyboardFocus();
            PaletteToggle.Unfocused += (_, _) => FocusRing.IsVisible = false;
        }

        //A click also focuses the switch, and a ring left behind after every click would be noise. Windows tells
        //the two apart; elsewhere any focus shows the ring.
        private bool HasKeyboardFocus()
        {
#if WINDOWS
            return PaletteToggle.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Control { FocusState: Microsoft.UI.Xaml.FocusState.Keyboard };
#else
            return true;
#endif
        }

        private void OnPaletteToggled(object? sender, ToggledEventArgs e)
        {
            PaletteService.Apply(e.Value ? PaletteStyle.Girl : PaletteStyle.Boy);
            UpdateDescription();
        }

        //The symbols carry no text, so the screen reader gets the palette by name
        private void UpdateDescription()
        {
            SemanticProperties.SetDescription(PaletteToggle, $"Color palette: {PaletteService.Current}");
        }
    }
}
