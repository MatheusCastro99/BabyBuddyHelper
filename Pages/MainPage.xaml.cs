using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper
{
    public partial class MainPage : ContentPage
    {
        private readonly DateTime expectedDueDate = new(2027, 02, 02, 0, 0, 0, DateTimeKind.Local);
        private readonly ObservableCollection<BabyModel> _emptyBabyProfiles = new();
        private IBabyProfileService? _babyProfileService;
        private bool _isSubscribedToBabyProfiles;
        private DateTime currentDate;

        public int MonthsUntilDue { get; private set; }
        public int DaysUntilDue { get; private set; }
        public string CountdownText { get; private set; } = "";
        public ObservableCollection<BabyModel> BabyProfiles { get; private set; }
        public bool HasBabyProfiles => BabyProfiles.Count > 0;
        public bool HasNoBabyProfiles => !HasBabyProfiles;
        public string ProfileCountText => BabyProfiles.Count switch
        {
            0 => "No profiles yet",
            1 => "1 little profile",
            _ => $"{BabyProfiles.Count} little profiles"
        };

        public MainPage()
        {
            InitializeComponent();

            BabyProfiles = _emptyBabyProfiles;
            HandleCounter(); //Starts counter

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!TryResolveBabyProfileService())
            {
                return;
            }

            RefreshBabyProfileState();
        }

        private bool TryResolveBabyProfileService()
        {
            if (_babyProfileService is not null)
            {
                return true;
            }

            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
            var babyProfileService = serviceProvider?.GetService<IBabyProfileService>();

            if (babyProfileService is null)
            {
                return false;
            }

            _babyProfileService = babyProfileService;
            BabyProfiles = babyProfileService.BabyProfiles;

            if (!_isSubscribedToBabyProfiles)
            {
                BabyProfiles.CollectionChanged += (_, _) => RefreshBabyProfileState();
                _isSubscribedToBabyProfiles = true;
            }

            return true;
        }

        private void HandleCounter()
        {
            currentDate = DateTime.Now;

            if (currentDate >= expectedDueDate) //Handles dates after specified due date
            {
                // Calculate months and days since birth
                MonthsUntilDue = (currentDate.Year - expectedDueDate.Year) * 12 + (currentDate.Month - expectedDueDate.Month);
                DaysUntilDue = currentDate.Day - expectedDueDate.Day;

                // Handle negative days
                if (DaysUntilDue < 0)
                {
                    MonthsUntilDue--;
                    DaysUntilDue += DateTime.DaysInMonth(expectedDueDate.Year, expectedDueDate.Month);
                }

                CountdownText = $"Welcome Baby! You are {MonthsUntilDue} Months and {DaysUntilDue} Days old";
            }

            else // Handles dates before due date
            {
                // Calculate months and remaining days
                MonthsUntilDue = (expectedDueDate.Year - currentDate.Year) * 12 + (expectedDueDate.Month - currentDate.Month);
                DaysUntilDue = expectedDueDate.Day - currentDate.Day;

                // Handle negative days by adjusting months
                if (DaysUntilDue < 0)
                {
                    MonthsUntilDue--;
                    DaysUntilDue += DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                }

                // Create formatted text
                CountdownText = $"{MonthsUntilDue} Months and {DaysUntilDue} Days until new Baby drops in!";
            }
        }

        private async void OnAddBabyProfileClicked(object? sender, EventArgs e)
        {
            if (!TryResolveBabyProfileService())
            {
                return;
            }

            await Navigation.PushModalAsync(new AddBabyPage(_babyProfileService!));
        }

        private async void OnEditBabyProfileClicked(object? sender, EventArgs e)
        {
            if (!TryResolveBabyProfileService() || sender is not Button { BindingContext: BabyModel babyProfile })
            {
                return;
            }

            await Navigation.PushModalAsync(new AddBabyPage(_babyProfileService!, babyProfile));
        }

        private async void OnDeleteBabyProfileClicked(object? sender, EventArgs e)
        {
            if (!TryResolveBabyProfileService() || sender is not Button { BindingContext: BabyModel babyProfile })
            {
                return;
            }

            bool shouldDelete = await DisplayAlertAsync(
                "Delete profile?",
                $"Remove {babyProfile.Name}'s profile? You can always add it back later.",
                "Delete",
                "Keep");

            if (!shouldDelete)
            {
                return;
            }

            _babyProfileService.Remove(babyProfile);
            RefreshBabyProfileState();
        }

        private void RefreshBabyProfileState()
        {
            OnPropertyChanged(nameof(BabyProfiles));
            OnPropertyChanged(nameof(HasBabyProfiles));
            OnPropertyChanged(nameof(HasNoBabyProfiles));
            OnPropertyChanged(nameof(ProfileCountText));
        }
    }
}
