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

        enum CompanionAnimation
        {
            Scale,
            Rotate,
            Bounce,
            Sway,
            CircleAround,
        }

        static readonly CompanionAnimation[] AnimationPool =
        [
            CompanionAnimation.Scale,
            CompanionAnimation.Rotate,
            CompanionAnimation.Bounce,
            CompanionAnimation.Sway,
            CompanionAnimation.CircleAround,
        ];

        IDispatcherTimer? _refreshTimer;
        bool _hasPlayedInitialGreeting;
        bool _isAnimationRunning;

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
            await PlayRandomAnimationAsync();
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

        async Task PlayRandomAnimationAsync()
        {
            // IDispatcherTimer.Tick is not await-aware. Without this guard, a timer
            // tick can start another animation while the previous one is still running.
            if (_isAnimationRunning)
            {
                return;
            }

            _isAnimationRunning = true;

            try
            {
                // Play exactly one lightweight animation for each new tip.
                switch (AnimationPool[Random.Shared.Next(AnimationPool.Length)])
                {
                    case CompanionAnimation.Scale:
                        await PlayScaleAnimationAsync();
                        break;
                    case CompanionAnimation.Rotate:
                        await PlayRotateAnimationAsync();
                        break;
                    case CompanionAnimation.Bounce:
                        await PlayBounceAnimationAsync();
                        break;
                    case CompanionAnimation.Sway:
                        await PlaySwayAnimationAsync();
                        break;
                    case CompanionAnimation.CircleAround:
                        await PlayCircleAroundAnimationAsync();
                        break;
                }
            }
            finally
            {
                // A detached view may cancel an animation while it is awaiting.
                // Restore the baseline for the next handler attachment.
                CubBadge.CancelAnimations();
                CubBadge.Scale = 1.0;
                CubBadge.Rotation = 0;
                CubBadge.TranslationX = 0;
                CubBadge.TranslationY = 0;
                _isAnimationRunning = false;
            }
        }

        async Task PlayScaleAnimationAsync()
        {
            await CubBadge.ScaleToAsync(1.12, 120, Easing.CubicOut);
            await CubBadge.ScaleToAsync(1.0, 120, Easing.CubicIn);
        }

        async Task PlayRotateAnimationAsync()
        {
            await CubBadge.RotateToAsync(-25, 140, Easing.CubicOut);
            await CubBadge.RotateToAsync(25, 220, Easing.SinInOut);
            await CubBadge.RotateToAsync(0, 140, Easing.CubicIn);
        }

        async Task PlayBounceAnimationAsync()
        {
            await CubBadge.TranslateToAsync(0, -10, 110, Easing.CubicOut);
            await CubBadge.TranslateToAsync(0, 0, 110, Easing.CubicIn);
            await CubBadge.TranslateToAsync(0, 5, 80, Easing.CubicOut);
            await CubBadge.TranslateToAsync(0, 0, 80, Easing.CubicIn);
        }

        async Task PlaySwayAnimationAsync()
        {
            await CubBadge.TranslateToAsync(-6, 0, 120, Easing.CubicOut);
            await CubBadge.TranslateToAsync(6, 0, 240, Easing.SinInOut);
            await CubBadge.TranslateToAsync(0, 0, 120, Easing.CubicIn);
        }

        async Task PlayCircleAroundAnimationAsync()
        {
            // Approximate a small circle with short translation segments. Keeping
            // the offsets small makes this feel like a playful flip, not a jump.
            await CubBadge.TranslateToAsync(4, -4, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(6, 0, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(4, 4, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(0, 6, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(-4, 4, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(-6, 0, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(-4, -4, 80, Easing.SinInOut);
            await CubBadge.TranslateToAsync(0, 0, 80, Easing.SinInOut);
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null)
            {
                StopRefreshTimer();
                CubBadge.CancelAnimations();
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
