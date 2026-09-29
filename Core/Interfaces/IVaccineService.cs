using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Core.Interfaces
{
    public interface IVaccineService //Cache service and single owner of vaccination records (ADR-001/008)
    {
        ObservableCollection<VaccinationRecordModel> VaccinationRecords { get; }
        Task InitializeAsync();
        IReadOnlyList<VaccinationRecordModel> GetRecordsForBaby(Guid babyId);
        Task AddAsync(VaccinationRecordModel record);
        Task UpdateAsync(VaccinationRecordModel record);
        Task RemoveAsync(Guid recordId);
    }
}
