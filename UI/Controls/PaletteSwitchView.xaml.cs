using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Switches the app between the boy and girl palettes. It sits on the Shell tab row, at the far right,
    /// so it is reachable from every tab without living on any page.
    /// </summary>
    public partial class PaletteSwitchView : ContentView
    {
        public PaletteSwitchView()
        {
            InitializeComponent();

            //Temporary test hook until the real switch lands: a tap flips the palette
            TapGestureRecognizer tap = new();
            tap.Tapped += (_, _) => PaletteService.Apply(PaletteService.Current == PaletteStyle.Boy ? PaletteStyle.Girl : PaletteStyle.Boy);
            GestureRecognizers.Add(tap);
        }
    }
}
