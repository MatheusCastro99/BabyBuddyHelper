using BabyBuddyHelper.Core.Models.Baby;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Core.Interfaces.Baby
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
