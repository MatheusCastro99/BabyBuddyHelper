using BabyBuddyHelper.Services;

namespace BabyBuddyHelper.Controls
{
    /// <summary>
    /// Non-intrusive toast used to give short, companion-themed feedback
    /// after task and appointment actions (completed, added, edited,
    /// deleted, scheduled). It listens to <see cref="ToastService"/> while
    /// attached to the visual tree and fades itself in/out, never blocking
    /// the primary workflow.
    /// </summary>
    public partial class ToastView : ContentView
    {
        static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(3);
        const double HiddenTranslationY = 16;

        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(
                nameof(Message),
                typeof(string),
                typeof(ToastView),
                defaultValue: string.Empty);

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        CancellationTokenSource? _hideCts;

        public ToastView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            // Guard against double-subscription (e.g. handler re-attached
            // without a prior detach) by always unsubscribing first.
            ToastService.ToastRequested -= OnToastRequested;

            if (Handler is null)
            {
                _hideCts?.Cancel();
                return;
            }

            ToastService.ToastRequested += OnToastRequested;
        }

        void OnToastRequested(object? sender, ToastRequestedEventArgs e) =>
            _ = ShowAsync(e.Message);

        async Task ShowAsync(string message)
        {
            // A newer toast should replace whatever is currently showing
            // instead of queueing behind it.
            _hideCts?.CancelAsync();
            var cts = new CancellationTokenSource();
            _hideCts = cts;

            Message = message;
            IsVisible = true;

            this.CancelAnimations();
            TranslationY = HiddenTranslationY;

            await Task.WhenAll(
                this.FadeToAsync(1, 160, Easing.CubicOut),
                this.TranslateToAsync(0, 0, 160, Easing.CubicOut));

            await PlayGreetingBounceAsync();

            try
            {
                await Task.Delay(DisplayDuration, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return; // a newer toast has already taken over
            }

            if (cts.IsCancellationRequested)
            {
                return;
            }

            await Task.WhenAll(
                this.FadeToAsync(0, 160, Easing.CubicIn),
                this.TranslateToAsync(0, HiddenTranslationY, 160, Easing.CubicIn));

            IsVisible = false;
        }

        async Task PlayGreetingBounceAsync()
        {
            await ToastBadge.ScaleToAsync(1.15, 110, Easing.CubicOut);
            await ToastBadge.ScaleToAsync(1.0, 110, Easing.CubicIn);
        }
    }
}
