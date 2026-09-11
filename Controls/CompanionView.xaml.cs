namespace BabyBuddyHelper.Controls
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
        ];

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

        static string PickRandomMessage() =>
            DefaultMessages[Random.Shared.Next(DefaultMessages.Length)];

        async Task PlayGreetingAnimationAsync()
        {
            // Short, subtle bounce to draw gentle attention without overwhelming the user.
            await CubBadge.ScaleToAsync(1.08, 90, Easing.CubicOut);
            await CubBadge.ScaleToAsync(1.0, 90, Easing.CubicIn);
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is not null)
            {
                _ = PlayGreetingAnimationAsync();
            }
        }
    }
}
