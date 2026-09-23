namespace BabyBuddyHelper.Services
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
    }
}
