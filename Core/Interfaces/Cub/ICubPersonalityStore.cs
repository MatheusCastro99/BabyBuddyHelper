using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Core.Interfaces.Cub
{
    public interface ICubPersonalityStore //How Cub talks in the chat (#97): a device setting, like the palette, never in the database
    {
        //The saved choice, or the default for any half that is missing or unknown
        CubPersonality Load();

        void Save(CubPersonality personality);
    }
}
