using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Switches the app between the boy and girl palettes (off = boy, on = girl). On Windows it sits on the Shell
    /// tab row, at the far right, so it is reachable from every tab (ShellTabBarFooter). On Android and the other
    /// platforms, which have no such slot, it sits at the top right of the Home greeting card.
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

            KeyboardFocusRing.Attach(PaletteToggle, FocusRing);
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
