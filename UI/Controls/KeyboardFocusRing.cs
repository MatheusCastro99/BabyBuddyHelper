namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Focus indicator for the drawn switches. Their real Switch is invisible and can't show its own focus ring,
    /// so a ring drawn around the track stands in for it.
    /// </summary>
    public static class KeyboardFocusRing
    {
        /// <summary>
        /// Shows <paramref name="ring"/> while <paramref name="input"/> has keyboard focus.
        /// </summary>
        public static void Attach(VisualElement input, VisualElement ring)
        {
            input.Focused += (_, _) => ring.IsVisible = HasKeyboardFocus(input);
            input.Unfocused += (_, _) => ring.IsVisible = false;
        }

        //A click also focuses the switch, and a ring left behind after every click would be noise. Windows tells
        //the two apart; elsewhere any focus shows the ring.
        private static bool HasKeyboardFocus(VisualElement input)
        {
#if WINDOWS
            return input.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Control { FocusState: Microsoft.UI.Xaml.FocusState.Keyboard };
#else
            return true;
#endif
        }
    }
}
