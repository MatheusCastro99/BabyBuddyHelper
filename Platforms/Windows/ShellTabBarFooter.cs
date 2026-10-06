using BabyBuddyHelper.UI.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace BabyBuddyHelper.WinUI
{
    /// <summary>
    /// Puts the palette switch on the Shell tab row, in line with the tabs and at the far right.
    /// Shell has no slot for this, so it goes into the footer of the native NavigationView that draws the tabs.
    /// </summary>
    public static class ShellTabBarFooter
    {
        public static void Register()
        {
            ShellItemHandler.Mapper.AppendToMapping(nameof(PaletteSwitchView), (handler, _) =>
            {
                //The mapping can run again for the same native view, so the footer is only filled once
                if (handler.PlatformView is NavigationView { PaneFooter: null } tabRow && handler.MauiContext is { } context)
                {
                    tabRow.PaneFooter = new PaletteSwitchView().ToPlatform(context);
                }
            });
        }
    }
}
