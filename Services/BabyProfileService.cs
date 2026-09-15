using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Services
{
    public class BabyProfileService : IBabyProfileService
    {
        public ObservableCollection<BabyModel> BabyProfiles { get; } = new();

        public void Add(BabyModel babyProfile)
        {
            BabyProfiles.Add(babyProfile);
        }

        public void Remove(BabyModel babyProfile)
        {
            BabyProfiles.Remove(babyProfile);
        }

        public void Update(BabyModel babyProfile)
        {
            var existingProfile = BabyProfiles.FirstOrDefault(x => x.Id == babyProfile.Id);

            if (existingProfile is null)
                return;

            var index = BabyProfiles.IndexOf(existingProfile);
            BabyProfiles[index] = babyProfile;
        }
    }
}
