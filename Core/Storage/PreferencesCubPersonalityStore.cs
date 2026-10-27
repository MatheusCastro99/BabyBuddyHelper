using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Models.Cub;

namespace BabyBuddyHelper.Core.Storage
{
    //Keeps the parent's choice of Cub's style and reply length in Preferences, saved by name (#97).
    //It is a device setting, like the palette, so it needs no migration. CubPersonality.FromSaved handles the fallback.
    public sealed class PreferencesCubPersonalityStore : ICubPersonalityStore
    {
        private const string StyleKey = "cub_style";
        private const string VerbosityKey = "cub_verbosity";

        public CubPersonality Load() =>
            CubPersonality.FromSaved(Preferences.Default.Get<string?>(StyleKey, null), Preferences.Default.Get<string?>(VerbosityKey, null));

        public void Save(CubPersonality personality)
        {
            Preferences.Default.Set(StyleKey, personality.Style.ToString());
            Preferences.Default.Set(VerbosityKey, personality.Verbosity.ToString());
        }
    }
}
