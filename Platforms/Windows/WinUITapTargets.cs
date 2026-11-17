using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using Thickness = Microsoft.UI.Xaml.Thickness;
using GridLength = Microsoft.UI.Xaml.GridLength;
using Grid = Microsoft.UI.Xaml.Controls.Grid;
using GridUnitType = Microsoft.UI.Xaml.GridUnitType;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace BabyBuddyHelper.WinUI
{
    /// <summary>
    /// Brings every native Switch and CheckBox to a hit area of at least 48x48 DIP on Windows.
    /// A `WidthRequest` or `HeightRequest` does not do it: the control gets that size, but its template draws and
    /// reports a smaller area inside it (the CheckBox's root grid keeps a 6 DIP margin, the ToggleSwitch's grid is
    /// 40 DIP tall), and UI Automation and the pointer use the template's area. So the template's own grid is
    /// resized once it exists.
    /// Windows only: Android's controls already meet 48 dp.
    /// </summary>
    public static class WinUITapTargets
    {
        //48 DIP plus 1: at a fractional pixel offset (125% display scale) pixel snapping can take a 48 DIP control to 59 of 60 px
        private const double MinimumSize = 49;

        public static void Register()
        {
            SwitchHandler.Mapper.AppendToMapping(nameof(WinUITapTargets), (handler, view) =>
                WhenLoaded(handler.PlatformView, FillSwitch));

            CheckBoxHandler.Mapper.AppendToMapping(nameof(WinUITapTargets), (handler, view) =>
                WhenLoaded(handler.PlatformView, FillCheckBox));
        }

        //The template is only built once the control is in the tree
        private static void WhenLoaded(FrameworkElement control, Action<FrameworkElement> resize)
        {
            if (control.IsLoaded)
            {
                resize(control);
                return;
            }

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                control.Loaded -= OnLoaded;
                resize(control);
            }

            control.Loaded += OnLoaded;
        }

        //Template: ToggleSwitch > Grid > Grid (the switch: columns Auto/0/Auto, rows 10/Auto/10, so 40 DIP wide and
        //40.8 tall, with the Thumb spanning all of them). Stretching the outer rows and the first column lets the
        //Thumb, which is what UI Automation reports, take the whole control
        private static void FillSwitch(FrameworkElement control)
        {
            control.MinWidth = MinimumSize;
            control.MinHeight = MinimumSize;
            if (FirstChild(FirstChild(control)) is Grid { ColumnDefinitions.Count: 3, RowDefinitions.Count: 3 } switchGrid)
            {
                switchGrid.MinWidth = MinimumSize;
                switchGrid.MinHeight = MinimumSize;
                switchGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                switchGrid.VerticalAlignment = VerticalAlignment.Stretch;
                switchGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                switchGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                switchGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            }
        }

        //Template: CheckBox > RootGrid (6 DIP margin, a 32 DIP glyph column and the content column)
        private static void FillCheckBox(FrameworkElement control)
        {
            control.MinWidth = MinimumSize;
            control.MinHeight = MinimumSize;
            if (FirstChild(control) is Grid { ColumnDefinitions.Count: > 0 } root)
            {
                root.Margin = new Thickness(0);
                root.ColumnDefinitions[0].Width = new GridLength(MinimumSize);
            }
        }

        private static DependencyObject? FirstChild(DependencyObject? parent) =>
            parent is not null && VisualTreeHelper.GetChildrenCount(parent) > 0 ? VisualTreeHelper.GetChild(parent, 0) : null;
    }
}
