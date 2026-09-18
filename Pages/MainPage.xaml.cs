using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Pages;
using BabyBuddyHelper.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BabyBuddyHelper
{
    public partial class MainPage : ContentPage
    {
        private readonly DateTime expectedDueDate = new(2027, 02, 02, 0, 0, 0, DateTimeKind.Local);
        private readonly ITaskListService _taskListService;
        private readonly IBabyProfileService _babyProfileService;
        private DateTime currentDate;

        public int MonthsUntilDue { get; private set; }
        public int DaysUntilDue { get; private set; }
        public string CountdownText { get; private set; } = "";
        public ObservableCollection<BabyModel> BabyProfiles { get; }
        public bool HasBabyProfiles => BabyProfiles.Count > 0;
        public bool HasNoBabyProfiles => !HasBabyProfiles;
        public string ProfileCountText => BabyProfiles.Count switch
        {
            0 => "No profiles yet",
            1 => "1 little profile",
            _ => $"{BabyProfiles.Count} little profiles"
        };

        public string WeeklyAppointmentText => CountAppointmentsThisWeek() switch
        {
            0 => "Nothing scheduled",
            1 => "1 appointment",
            var count => $"{count} appointments"
        };

        public string CompletedTaskText => _taskListService.Tasks.Count(task => task.IsCompleted) switch
        {
            0 => "None yet",
            1 => "1 done",
            var count => $"{count} done"
        };

        public MainPage(ITaskListService taskListService, IBabyProfileService babyProfileService)
        {
            InitializeComponent();

            _taskListService = taskListService;
            _babyProfileService = babyProfileService;
            BabyProfiles = babyProfileService.BabyProfiles;

            _taskListService.Tasks.CollectionChanged += (_, _) => RefreshDashboardState();
            BabyProfiles.CollectionChanged += (_, _) => RefreshDashboardState();

            HandleCounter(); //Starts counter

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Completing a task mutates the model in place without touching the collection, so returning to this
            //tab is what brings the completion count back in sync.
            RefreshDashboardState();
        }

        private int CountAppointmentsThisWeek()
        {
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DateTime today = DateTime.Today;

            DateTime weekStart = today.AddDays(-(((int)today.DayOfWeek - (int)firstDayOfWeek + 7) % 7));
            DateTime weekEnd = weekStart.AddDays(6);

            return _taskListService.GetAppointments()
                .Count(appointment => appointment.AppointmentDate.HasValue
                    && appointment.AppointmentDate.Value.Date >= weekStart
                    && appointment.AppointmentDate.Value.Date <= weekEnd);
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
            await Navigation.PushModalAsync(new AddBabyPage(_babyProfileService));
        }

        private async void OnEditBabyProfileClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: BabyModel babyProfile })
            {
                return;
            }

            await Navigation.PushModalAsync(new AddBabyPage(_babyProfileService, babyProfile));
        }

        private async void OnDeleteBabyProfileClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: BabyModel babyProfile })
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

            await _babyProfileService.RemoveAsync(babyProfile.Id);
            RefreshDashboardState();
            ToastService.Show(ToastKind.BabyProfileDeleted);
        }

        private void RefreshDashboardState()
        {
            OnPropertyChanged(nameof(BabyProfiles));
            OnPropertyChanged(nameof(HasBabyProfiles));
            OnPropertyChanged(nameof(HasNoBabyProfiles));
            OnPropertyChanged(nameof(ProfileCountText));
            OnPropertyChanged(nameof(WeeklyAppointmentText));
            OnPropertyChanged(nameof(CompletedTaskText));
        }
    }
}
