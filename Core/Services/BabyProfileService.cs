using BabyBuddyHelper.Core.Collections;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Core.Services
{
    //In-memory cache of baby profiles. Writes persist through ITrackerDbService first; the collection only changes once the
    //database has committed, so a failed save (DbCommunicationException) leaves it untouched. Entries are found again by Id after the await (ADR-007).
    public class BabyProfileService : IBabyProfileService
    {
        private readonly ITrackerDbService _trackerDbService;
        private Task? _initializationTask;
        private readonly RangeObservableCollection<BabyModel> _babyProfiles = new();
        public ObservableCollection<BabyModel> BabyProfiles => _babyProfiles;

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

        //One Reset for the whole load. TaskListService treats a Reset as "clear links to missing babies"; that is harmless here
        //because profiles load before tasks (see App.RunStartupAsync), so the task list is still empty.
        private async Task LoadBabyProfilesAsync()
        {
            _babyProfiles.AddRange(await _trackerDbService.GetBabyProfilesAsync());
        }

        public async Task AddAsync(BabyModel babyProfile)
        {
            await _trackerDbService.AddBabyProfileAsync(babyProfile);
            BabyProfiles.Add(babyProfile);
        }

        //The database clears this baby from its tasks first (ON DELETE SET NULL). Removing the profile from the cache afterwards
        //raises CollectionChanged, and TaskListService mirrors that by clearing the same links in memory.
        public async Task RemoveAsync(Guid babyId)
        {
            if (!BabyProfiles.Any(x => x.Id == babyId))
                return;

            await _trackerDbService.RemoveBabyProfileAsync(babyId);

            var removedProfile = BabyProfiles.FirstOrDefault(x => x.Id == babyId);

            if (removedProfile is not null)
            {
                BabyProfiles.Remove(removedProfile);
            }
        }

        public async Task UpdateAsync(BabyModel babyProfile)
        {
            if (!BabyProfiles.Any(x => x.Id == babyProfile.Id))
                return;

            await _trackerDbService.UpdateBabyProfileAsync(babyProfile);

            var existingProfile = BabyProfiles.FirstOrDefault(x => x.Id == babyProfile.Id);

            if (existingProfile is not null)
            {
                BabyProfiles[BabyProfiles.IndexOf(existingProfile)] = babyProfile;
            }
        }
    }
}
