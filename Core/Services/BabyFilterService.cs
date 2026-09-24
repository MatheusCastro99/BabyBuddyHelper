using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;

namespace BabyBuddyHelper.Services
{
    public class BabyFilterService : IBabyFilterService //Builds the baby option lists consumed by ChecklistPage, CalendarPage, and AddTaskPage.
    {
        private readonly IBabyProfileService _babyProfileService;

        public BabyFilterService(IBabyProfileService babyProfileService)
        {
            _babyProfileService = babyProfileService;
        }

        public Dictionary<string, Guid?> BuildOptions(string firstOptionLabel)
        {
            Dictionary<string, Guid?> options = new()
            {
                [firstOptionLabel] = null
            };

            foreach (BabyModel profile in _babyProfileService.BabyProfiles)
            {
                string optionLabel = profile.Name;
                int duplicateCounter = 2;

                while (options.ContainsKey(optionLabel)) //Keeps duplicate baby names distinguishable in the option list
                {
                    optionLabel = $"{profile.Name} ({duplicateCounter})";
                    duplicateCounter++;
                }

                options[optionLabel] = profile.Id;
            }

            return options;
        }

        public (Guid? ResolvedBabyId, string Label) ResolveSelection(Guid? selectedBabyId, string firstOptionLabel)
        {
            if (!selectedBabyId.HasValue)
            {
                return (null, firstOptionLabel);
            }

            string? selectedLabel = BuildOptions(firstOptionLabel)
                .Where(option => option.Value == selectedBabyId.Value)
                .Select(option => option.Key)
                .FirstOrDefault();

            //A null label means the selected profile was deleted, so the caller falls back to the default option
            return selectedLabel is null ? (null, firstOptionLabel) : (selectedBabyId, selectedLabel);
        }
    }
}
