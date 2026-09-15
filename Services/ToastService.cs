namespace BabyBuddyHelper.Services
{
    /// <summary>
    /// Categories of actions that can trigger companion-themed toast feedback.
    /// </summary>
    public enum ToastKind
    {
        TaskCompleted,
        TaskAdded,
        TaskEdited,
        TaskDeleted,
        AppointmentScheduled,
        BabyProfileCreated,
        BabyProfileUpdated,
    }

    /// <summary>
    /// Lightweight, static pub/sub hub that lets pages request non-intrusive
    /// toast feedback without needing a direct reference to a visible ToastView.
    /// Messages are phrased as short notes from Cub, the app's companion.
    /// </summary>
    public static class ToastService
    {
        static readonly Dictionary<ToastKind, string[]> MessagePool = new()
        {
            [ToastKind.TaskCompleted] =
            [
                "Cub cheers, another task wrapped up!",
                "Cub gives you a proud little nod, well done!",
                "Cub says: one more thing taken care of!",
                "Cub celebrates that little win with you!",
                "Cub says: nicely done, that's off your plate!",
            ],
            [ToastKind.TaskAdded] =
            [
                "Cub tucks a new task into your list.",
                "Cub says: got it, added to your list!",
                "Cub is keeping an eye on this new task with you.",
                "Cub says: one more detail organized for the day!",
                "Cub makes room for your new task, all set!",
            ],
            [ToastKind.TaskEdited] =
            [
                "Cub says: all updated and looking good!",
                "Cub tidies up the details for you.",
                "Cub gives a little nod, changes saved!",
                "Cub says: those details are refreshed and ready!",
                "Cub gives a happy nod to your thoughtful update!",
            ],
            [ToastKind.TaskDeleted] =
            [
                "Cub waves that task goodbye, one less thing to carry.",
                "Cub says: all cleared, nice and tidy!",
                "Cub gives you a little high-five for tidying up.",
                "Cub helps lighten the list, one task at a time.",
                "Cub says: cleared away and making room for what matters!",
            ],
            [ToastKind.AppointmentScheduled] =
            [
                "Cub marks the calendar, that appointment is set!",
                "Cub says: saved, you're all set for that visit.",
                "Cub is looking forward to that appointment with you.",
                "Cub says: that time is safely saved on the calendar!",
                "Cub adds the appointment to the plan, nice and easy!",
            ],
            [ToastKind.BabyProfileCreated] =
            [
                "Cub says: that little profile is all set!",
                "Cub carefully tucked that baby profile into place.",
                "Cub gives a happy wiggle, profile saved!",
                "Cub says: your baby's details are ready whenever you need them.",
                "Cub helps you keep that new profile close at hand!",
            ],
            [ToastKind.BabyProfileUpdated] =
            [
                "Cub says: that baby profile is refreshed and ready!",
                "Cub tidied up those profile details for you.",
                "Cub gives a warm little nod, changes saved!",
                "Cub says: those baby details are up to date now.",
                "Cub helps keep that profile current and cozy!",
            ],
        };

        public static event EventHandler<ToastRequestedEventArgs>? ToastRequested;

        /// <summary>
        /// Raises a toast request, picking one message at random from the
        /// pool associated with <paramref name="kind"/>.
        /// </summary>
        public static void Show(ToastKind kind)
        {
            var pool = MessagePool[kind];
            var message = pool[Random.Shared.Next(pool.Length)];

            ToastRequested?.Invoke(null, new ToastRequestedEventArgs(kind, message));
        }
    }

    public sealed class ToastRequestedEventArgs(ToastKind kind, string message) : EventArgs
    {
        public ToastKind Kind { get; } = kind;
        public string Message { get; } = message;
    }
}
