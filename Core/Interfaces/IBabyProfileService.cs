using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Core.Interfaces
{
    public interface IBabyProfileService
    {
        ObservableCollection<BabyModel> BabyProfiles { get; }
        Task InitializeAsync();
        Task AddAsync(BabyModel babyProfile);
        Task RemoveAsync(Guid babyId);
        Task UpdateAsync(BabyModel babyProfile);
    }
}
