namespace BabyBuddyHelper.UI.Services
{
    //Alerts for moments when the app didn't behave as expected and the user must acknowledge it. Confirmations and
    //encouragement stay in ToastService. Cub is deliberately absent here: the companion only appears in positive moments.
    public static class AlertService
    {
        //Native alerts can't style text, so the hint is set apart as its own paragraph rather than shown in smaller type
        private const string PersistenceHint = "If this keeps happening, restarting the app can help.";

        //A write that didn't reach the database. Nothing changed, so the message should say what is still safe.
        public static Task ShowWriteFailedAsync(Page page, string title, string message) =>
            page.DisplayAlertAsync(title, $"{message}\n\n{PersistenceHint}", "OK");

        //The startup load failed. Returns true for "Try again". No safety claim here: unlike a failed write, a failed read
        //can mean a damaged file, so the message doesn't promise the information is intact.
        public static async Task<bool> ShowStartupLoadFailedAsync(Page page)
        {
            await WaitUntilLoadedAsync(page);
            return await page.DisplayAlertAsync(
                "Couldn't load your information",
                "The app couldn't open your profiles, tasks and appointments just now. Please try again.",
                "Try again",
                "Continue anyway");
        }

        //After "Continue anyway". Explains the empty screens before the user sees them.
        public static async Task ShowRunningWithoutDataAsync(Page page)
        {
            await WaitUntilLoadedAsync(page);
            await page.DisplayAlertAsync(
                "The app isn't working as expected",
                "Until your information loads, the app will look empty and new changes may not be saved. Your saved information won't be touched. Closing and reopening the app is strongly recommended.",
                "OK");
        }

        //Startup runs from Window.Created, while the window is still opening. An alert raised before its page has loaded
        //may never appear, so the startup alerts wait for it.
        private static Task WaitUntilLoadedAsync(Page page)
        {
            if (page.IsLoaded)
            {
                return Task.CompletedTask;
            }

            TaskCompletionSource loaded = new();
            page.Loaded += OnLoaded;
            return loaded.Task;

            void OnLoaded(object? sender, EventArgs e)
            {
                page.Loaded -= OnLoaded;
                loaded.TrySetResult();
            }
        }
    }
}
