using Microsoft.Maui.Dispatching;

namespace BabyBuddyHelper.Controls
{
    /// <summary>
    /// Lightweight companion character (a friendly lion cub) that offers
    /// short, supportive tips and encouragement. Intended to live on
    /// MainPage only, per the current UI guidelines.
    /// </summary>
    public partial class CompanionView : ContentView
    {
        readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

        static readonly string[] DefaultMessages =
        [
            "A calm rhythm keeps things manageable. Keep the next task small and steady.",
            "You're doing great today, one gentle step at a time.",
            "Small wins add up. Take a breath before the next task.",
            "Everything looks steady right now, keep going!",
        ];

        IDispatcherTimer? _refreshTimer;
        bool _hasPlayedInitialGreeting;

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
        /// Updates the companion's message with a brief, gentle animation
        /// so feedback feels lightweight rather than chat-like.
        /// </summary>
        public async Task ShowTipAsync(string message)
        {
            Message = message;
            await PlayGreetingAnimationAsync();
        }

        Task ShowRandomTipAsync() =>
            ShowTipAsync(PickRandomMessage(Message));

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

        async Task PlayGreetingAnimationAsync()
        {
            // Short, subtle bounce to draw gentle attention without overwhelming the user.
            await CubBadge.ScaleToAsync(1.08, 90, Easing.CubicOut);
            await CubBadge.ScaleToAsync(1.0, 90, Easing.CubicIn);
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null)
            {
                StopRefreshTimer();
                return;
            }

            StartRefreshTimer();

            if (!_hasPlayedInitialGreeting)
            {
                _hasPlayedInitialGreeting = true;
                _ = ShowTipAsync(Message);
            }
        }

        void StartRefreshTimer()
        {
            if (_refreshTimer is not null || Dispatcher is null)
            {
                return;
            }

            _refreshTimer = Dispatcher.CreateTimer();
            _refreshTimer.Interval = RefreshInterval;
            _refreshTimer.Tick += OnRefreshTimerTick;
            _refreshTimer.Start();
        }

        void StopRefreshTimer()
        {
            if (_refreshTimer is null)
            {
                return;
            }

            _refreshTimer.Stop();
            _refreshTimer.Tick -= OnRefreshTimerTick;
            _refreshTimer = null;
        }

        async void OnRefreshTimerTick(object? sender, EventArgs e) =>
            await ShowRandomTipAsync();

        async void OnTellMeSomethingClicked(object? sender, EventArgs e) =>
            await ShowRandomTipAsync();
    }
}
