using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BabyBuddyHelper.UI.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly DateTime expectedDueDate = new(2027, 02, 02, 0, 0, 0, DateTimeKind.Local);
        private readonly ITaskListService _taskListService;
        private readonly IBabyProfileService _babyProfileService;
        private readonly IVaccineCatalog _vaccineCatalog;
        private readonly IVaccineService _vaccineService;
        private readonly IApiKeyStore _apiKeyStore;
        private DateTime currentDate;
        private bool _isNavigating;

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

        //Each baby's vaccine tag, only for babies with something due; the cards look theirs up through VaccineTagConverter
        public IReadOnlyDictionary<Guid, string> VaccineTagsByBabyId { get; private set; } = new Dictionary<Guid, string>();

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

        public MainPage(ITaskListService taskListService, IBabyProfileService babyProfileService, IVaccineCatalog vaccineCatalog, IVaccineService vaccineService, IApiKeyStore apiKeyStore)
        {
            InitializeComponent();

            _taskListService = taskListService;
            _babyProfileService = babyProfileService;
            _vaccineCatalog = vaccineCatalog;
            _vaccineService = vaccineService;
            _apiKeyStore = apiKeyStore;
            BabyProfiles = babyProfileService.BabyProfiles;

            _taskListService.Tasks.CollectionChanged += (_, _) => RefreshDashboardState();
            BabyProfiles.CollectionChanged += (_, _) => RefreshDashboardState();
            _vaccineService.VaccinationRecords.CollectionChanged += (_, _) => RefreshDashboardState(); //Saving a record replaces it, so edits land here too

            HandleCounter(); //Starts counter

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //TaskListService.SetCompletionAsync updates the cached task in place without raising CollectionChanged,
            //so returning to this tab is what brings the completion count back in sync.
            RefreshDashboardState();

            _ = Companion.GreetAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Companion.StopGreeting();
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

        //Editing and deleting live on the profile page (ADR-004); the card only opens it
        private async void OnBabyProfileTapped(object? sender, TappedEventArgs e) => await OpenProfileAsync(sender);

        private async void OnViewProfileClicked(object? sender, EventArgs e) => await OpenProfileAsync(sender);

        private async Task OpenProfileAsync(object? sender)
        {
            if (sender is not BindableObject { BindingContext: BabyModel babyProfile })
            {
                return;
            }

            await PushOnceAsync(() => new BabyProfilePage(_babyProfileService, _vaccineCatalog, _vaccineService, babyProfile.Id));
        }

        //The chat is a pushed page like the profile (ADR-016). It starts empty every time: the conversation isn't kept
        private async void OnTalkToCubRequested(object? sender, EventArgs e) => await PushOnceAsync(() => new ChatPage(_apiKeyStore));

        //A double-click, or a click on the button that also reaches the card, can ask twice. The flag covers the
        //push while it's in flight; the stack check covers a late second request after it landed.
        private async Task PushOnceAsync(Func<Page> createPage)
        {
            if (_isNavigating || Navigation.NavigationStack.Count > 1)
            {
                return;
            }

            _isNavigating = true;

            try
            {
                await Navigation.PushAsync(createPage());
            }
            finally
            {
                _isNavigating = false;
            }
        }

        //Wording approved in #81. Due today wins over due soon, and overdue isn't shown here: it stays on the profile as a gentle reminder.
        private Dictionary<Guid, string> BuildVaccineTags()
        {
            Dictionary<Guid, string> vaccineTagsByBabyId = [];

            foreach (IGrouping<Guid, VaccinationRecordModel> babyRecords in _vaccineService.VaccinationRecords.GroupBy(x => x.BabyId))
            {
                List<string> dueToday = GetVaccineNames(babyRecords.Where(x => x.IsDueToday));
                List<string> dueSoon = GetVaccineNames(babyRecords.Where(x => x.IsDueSoon));

                string? vaccineTag = dueToday.Count > 0 ? FormatVaccineTag("Due today", dueToday)
                    : dueSoon.Count > 0 ? FormatVaccineTag("Due soon", dueSoon)
                    : null;

                if (vaccineTag is not null)
                {
                    vaccineTagsByBabyId[babyRecords.Key] = vaccineTag;
                }
            }

            return vaccineTagsByBabyId;
        }

        //A record whose vaccine isn't in the catalog has no name to show, so it's skipped
        private List<string> GetVaccineNames(IEnumerable<VaccinationRecordModel> records) =>
            records.Select(x => _vaccineCatalog.GetById(x.VaccineId)?.Name)
                .OfType<string>()
                .ToList();

        private static string FormatVaccineTag(string label, List<string> vaccineNames) =>
            vaccineNames.Count == 1 ? $"{label}: {vaccineNames[0]}" : $"{label}: {vaccineNames.Count} vaccines · check baby profile";

        private void RefreshDashboardState()
        {
            VaccineTagsByBabyId = BuildVaccineTags();

            OnPropertyChanged(nameof(VaccineTagsByBabyId));
            OnPropertyChanged(nameof(BabyProfiles));
            OnPropertyChanged(nameof(HasBabyProfiles));
            OnPropertyChanged(nameof(HasNoBabyProfiles));
            OnPropertyChanged(nameof(ProfileCountText));
            OnPropertyChanged(nameof(WeeklyAppointmentText));
            OnPropertyChanged(nameof(CompletedTaskText));
        }
    }
}
