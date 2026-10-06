using SkiaSharp.Extended.UI.Controls;

namespace BabyBuddyHelper.UI.Controls
{
    public enum CubAnimation
    {
        Wave,
        Clap,
        Dance,
        Nod,
    }

    /// <summary>
    /// Cub, the companion character, drawn from one Lottie file (Resources/Raw/cub.json).
    /// The file holds every animation back to back, each starting and ending on the
    /// rest pose. This view sits on the rest pose and plays one animation at a time.
    /// </summary>
    public class CubView : ContentView
    {
        const double FramesPerSecond = 60;
        const string PlaybackHandle = "CubPlayback";

        readonly SKLottieView _lottie;
        CubAnimation? _playing;

        public CubView()
        {
            _lottie = new SKLottieView
            {
                Source = new SKFileLottieImageSource { File = "cub.json" },
                RepeatCount = 0,
                InputTransparent = true,
            };

            //The view's own frame loop would run the whole file from start to end.
            //It stays off: PlayAsync moves Progress through one animation instead.
            _lottie.IsAnimationEnabled = false;

            Content = _lottie;
        }

        //First frame and length of each animation inside cub.json.
        //These mirror SEGMENTS in tools/cub/generate_cub.py and must change together.
        static (int Start, int Length) GetFrames(CubAnimation animation) => animation switch
        {
            CubAnimation.Wave => (0, 100),
            CubAnimation.Clap => (120, 90),
            CubAnimation.Dance => (230, 120),
            CubAnimation.Nod => (370, 56),
            _ => throw new ArgumentOutOfRangeException(nameof(animation)),
        };

        /// <summary>
        /// Plays one animation and returns when Cub is back on the rest pose.
        /// A request for the animation already playing is ignored; a different
        /// one takes over.
        /// </summary>
        public Task PlayAsync(CubAnimation animation)
        {
            if (_playing == animation)
            {
                return Task.CompletedTask;
            }

            this.AbortAnimation(PlaybackHandle);
            _playing = animation;

            var (start, length) = GetFrames(animation);
            double startSeconds = start / FramesPerSecond;
            double endSeconds = (start + length) / FramesPerSecond;
            var completion = new TaskCompletionSource();

            new Animation(seconds => _lottie.Progress = TimeSpan.FromSeconds(seconds), startSeconds, endSeconds)
                .Commit(this, PlaybackHandle, rate: 16, length: (uint)(length / FramesPerSecond * 1000), easing: Easing.Linear,
                    finished: (_, cancelled) =>
                    {
                        if (!cancelled)
                        {
                            _playing = null;
                        }

                        completion.TrySetResult();
                    });

            return completion.Task;
        }

        /// <summary>
        /// Stops whatever is playing and puts Cub back on the rest pose.
        /// </summary>
        public void Stop()
        {
            this.AbortAnimation(PlaybackHandle);
            _playing = null;
            _lottie.Progress = TimeSpan.Zero;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null)
            {
                Stop();
            }
        }
    }
}
