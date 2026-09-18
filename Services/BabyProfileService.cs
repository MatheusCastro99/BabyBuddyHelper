using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Services
{
    //In-memory cache of baby profiles. Writes update the collection first so the UI responds instantly, then persist.
    public class BabyProfileService : IBabyProfileService
    {
        private readonly ITrackerDbService _trackerDbService;
        private Task? _initializationTask;
        public ObservableCollection<BabyModel> BabyProfiles { get; } = new();

        public BabyProfileService(ITrackerDbService trackerDbService)
        {
            _trackerDbService = trackerDbService;
        }

        //Loads the cache from the database once at startup. Concurrent callers share the same load, so a re-created window
        //can't duplicate entries; a failed load is retried on the next call instead of leaving the cache empty for the session.
        public Task InitializeAsync()
        {
            if (_initializationTask is null || _initializationTask.IsFaulted || _initializationTask.IsCanceled)
            {
                _initializationTask = LoadBabyProfilesAsync();
            }

            return _initializationTask;
        }

        private async Task LoadBabyProfilesAsync()
        {
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
