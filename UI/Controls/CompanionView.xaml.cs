namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Lightweight companion character (a friendly lion cub) that offers
    /// short, supportive tips and encouragement. Intended to live on
    /// MainPage only, per the current UI guidelines.
    /// </summary>
    public partial class CompanionView : ContentView
    {
        static readonly string[] DefaultMessages =
        [
            "A calm rhythm keeps things manageable. Keep the next task small and steady.",
            "You're doing great today, one gentle step at a time.",
            "Small wins add up. Take a breath before the next task.",
            "Everything looks steady right now, keep going!",
            "You don't have to do everything at once. One moment is enough.",
            "Your care and attention make a meaningful difference every day.",
            "A little progress is still progress. You're moving forward.",
            "Take things at your own pace. There is no need to rush this moment.",
            "You are creating a loving routine, one small step at a time.",
            "Pause for a breath. You have already handled so much today.",
            "The little things you do each day matter more than you know.",
            "Keep going gently. You are doing better than you think.",
            "One completed task can make the next one feel a little lighter.",
            "You bring patience, care, and warmth to every part of today.",
            "It is okay to take a quiet moment before the next task.",
            "You are not behind. You are caring for your family in your own rhythm.",
            "Every calm choice helps build a comfortable day.",
            "You can focus on what is in front of you right now.",
            "Your steady effort is making today more manageable.",
            "A fresh start can happen at any moment. Keep it simple.",
            "You are making progress, even when the day feels busy.",
            "Give yourself credit for all the care you share.",
            "There is strength in taking things one small step at a time.",
            "You are doing meaningful work, even during the ordinary moments.",
            "Let the next task be enough for now. You've got this.",
            "Your presence and care are what matter most.",
            "A gentle pace can still carry you a long way.",
            "Today does not need to be perfect to be a good day.",
            "You are building small moments of comfort throughout the day.",
            "Keep going with kindness for yourself and the people you care for.",
        ];

        //Lets the page settle before Cub waves, so the greeting is seen rather than lost in the page load
        static readonly TimeSpan GreetingDelay = TimeSpan.FromMilliseconds(600);

        CancellationTokenSource? _greetingCts;

        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(
                nameof(Message),
                typeof(string),
                typeof(CompanionView),
                defaultValue: string.Empty);

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public CompanionView()
        {
            InitializeComponent();
            BindingContext = this;

            if (string.IsNullOrWhiteSpace(Message))
            {
                Message = PickRandomMessage();
            }
        }

        /// <summary>
        /// Cub waves hello. MainPage calls this every time it appears, and a
        /// newer call replaces one that is still waiting out its short delay.
        /// </summary>
        public async Task GreetAsync()
        {
            var cts = new CancellationTokenSource();
            ReplaceGreetingCts(cts);

            try
            {
                await Task.Delay(GreetingDelay, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return; // a newer greeting took over, or the view went away
            }

            await Cub.PlayAsync(CubAnimation.Wave);
        }

        static string PickRandomMessage(string? currentMessage = null)
        {
            if (DefaultMessages.Length == 1)
            {
                return DefaultMessages[0];
            }

            string nextMessage;

            do
            {
                nextMessage = DefaultMessages[Random.Shared.Next(DefaultMessages.Length)];
            }
            while (nextMessage == currentMessage);

            return nextMessage;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null)
            {
                ReplaceGreetingCts(null);
            }
        }

        void ReplaceGreetingCts(CancellationTokenSource? newCts)
        {
            var previousCts = _greetingCts;
            _greetingCts = newCts;

            previousCts?.Cancel();
            previousCts?.Dispose();
        }

        //A new tip comes with a little dance, so asking Cub for one feels playful
        async void OnTellMeSomethingClicked(object? sender, EventArgs e)
        {
            Message = PickRandomMessage(Message);
            await Cub.PlayAsync(CubAnimation.Dance);
        }

        //Tapping Cub is a small extra with no function behind it: Cub just waves back
        async void OnCubTapped(object? sender, TappedEventArgs e) =>
            await Cub.PlayAsync(CubAnimation.Wave);
    }
}
