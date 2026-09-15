using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Interfaces
{
    public interface IBabyProfileService
    {
        ObservableCollection<BabyModel> BabyProfiles { get; }
        void Add(BabyModel babyProfile);
        void Remove(BabyModel babyProfile);
        void Update(BabyModel babyProfile);
    }
}
