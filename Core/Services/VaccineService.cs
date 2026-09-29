using BabyBuddyHelper.Core.Collections;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BabyBuddyHelper.Core.Services
{
    //In-memory cache of vaccination records. Writes persist through ITrackerDbService first; the collection only changes once the
    //database has committed, so a failed save (DbCommunicationException) leaves it untouched. Entries are found again by Id after the await (ADR-007).
    public class VaccineService : IVaccineService, IDisposable
    {
        private readonly IBabyProfileService _babyProfileService;
        private readonly ITrackerDbService _trackerDbService;
        private Task? _initializationTask;
        private readonly RangeObservableCollection<VaccinationRecordModel> _vaccinationRecords = new();
        public ObservableCollection<VaccinationRecordModel> VaccinationRecords => _vaccinationRecords;

        public VaccineService(IBabyProfileService babyProfileService, ITrackerDbService trackerDbService)
        {
            _babyProfileService = babyProfileService;
            _trackerDbService = trackerDbService;
            babyProfileService.BabyProfiles.CollectionChanged += OnBabyProfilesChanged;
        }

        //Loads the cache from the database once at startup. Concurrent callers share the same load, so a re-created window
        //can't duplicate entries; a failed load is retried on the next call instead of leaving the cache empty for the session.
        public Task InitializeAsync()
        {
            if (_initializationTask is null || _initializationTask.IsFaulted || _initializationTask.IsCanceled)
            {
                _initializationTask = LoadVaccinationRecordsAsync();
            }

            return _initializationTask;
        }

        private async Task LoadVaccinationRecordsAsync()
        {
            _vaccinationRecords.AddRange(await _trackerDbService.GetVaccinationRecordsAsync());
        }

        public IReadOnlyList<VaccinationRecordModel> GetRecordsForBaby(Guid babyId) =>
            VaccinationRecords.Where(x => x.BabyId == babyId).ToList();

        //A second record for the same baby and vaccine is rejected by the database's unique index (DbCommunicationException)
        public async Task AddAsync(VaccinationRecordModel record)
        {
            await _trackerDbService.AddVaccinationRecordAsync(record);
            VaccinationRecords.Add(record);
        }

        public async Task UpdateAsync(VaccinationRecordModel record)
        {
            if (!VaccinationRecords.Any(x => x.Id == record.Id))
                return;

            await _trackerDbService.UpdateVaccinationRecordAsync(record);

            var existingRecord = VaccinationRecords.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord is not null)
            {
                VaccinationRecords[VaccinationRecords.IndexOf(existingRecord)] = record;
            }
        }

        public async Task RemoveAsync(Guid recordId)
        {
            if (!VaccinationRecords.Any(x => x.Id == recordId))
                return;

            await _trackerDbService.RemoveVaccinationRecordAsync(recordId);

            var removedRecord = VaccinationRecords.FirstOrDefault(x => x.Id == recordId);
            if (removedRecord is not null)
            {
                VaccinationRecords.Remove(removedRecord);
            }
        }

        //Removals only update the in-memory copy. By the time a profile leaves the cache, the database has already deleted its
        //records (ON DELETE CASCADE, see TrackerContext), so this mirrors a change that is already saved.
        //Reset is not handled: the only Reset is the profile load, which runs before records load (see App.RunStartupAsync).
        private void OnBabyProfilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove || e.OldItems is null)
                return;

            foreach (BabyModel removedProfile in e.OldItems.OfType<BabyModel>())
            {
                foreach (VaccinationRecordModel record in GetRecordsForBaby(removedProfile.Id))
                {
                    VaccinationRecords.Remove(record);
                }
            }
        }

        public void Dispose()
        {
            _babyProfileService.BabyProfiles.CollectionChanged -= OnBabyProfilesChanged;
        }
    }
}
