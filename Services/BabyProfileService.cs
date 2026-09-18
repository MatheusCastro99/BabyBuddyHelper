using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Services
{
    //In-memory cache of baby profiles. Writes update the collection first so the UI responds instantly, then persist.
    public class BabyProfileService : IBabyProfileService
    {
        private readonly ITrackerDbService _trackerDbService;
        private bool _isInitialized;
        public ObservableCollection<BabyModel> BabyProfiles { get; } = new();

        public BabyProfileService(ITrackerDbService trackerDbService)
        {
            _trackerDbService = trackerDbService;
        }

        //Loads the cache from the database once at startup. Later calls are ignored, so a re-created window can't duplicate entries.
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            foreach (BabyModel babyProfile in await _trackerDbService.GetBabyProfilesAsync())
            {
                BabyProfiles.Add(babyProfile);
            }
        }

        public async Task AddAsync(BabyModel babyProfile)
        {
            BabyProfiles.Add(babyProfile);
            await _trackerDbService.AddBabyProfileAsync(babyProfile);
        }

        public async Task RemoveAsync(Guid babyId)
        {
            var existingProfile = BabyProfiles.FirstOrDefault(x => x.Id == babyId);

            if (existingProfile is null)
                return;

            BabyProfiles.Remove(existingProfile);
            await _trackerDbService.RemoveBabyProfileAsync(babyId); //The database also clears this baby from its tasks
        }

        public async Task UpdateAsync(BabyModel babyProfile)
        {
            var existingProfile = BabyProfiles.FirstOrDefault(x => x.Id == babyProfile.Id);

            if (existingProfile is null)
                return;

            var index = BabyProfiles.IndexOf(existingProfile);
            BabyProfiles[index] = babyProfile;
            await _trackerDbService.UpdateBabyProfileAsync(babyProfile);
        }
    }
}
