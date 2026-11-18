using BabyBuddyHelper.Core.Interfaces.Baby;
using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Interfaces.TaskList;
using BabyBuddyHelper.Core.Interfaces.Vaccine;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Vaccine;
using BabyBuddyHelper.Core.Services.Cub;
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
        private readonly CubReplyService _cubReplyService;
        private readonly ICubPersonalityStore _personalityStore;
        private readonly IBabyFilterService _babyFilterService;
        private DateTime currentDate;
        private bool _isNavigating;
        private Window? _resumedWindow;

        public int MonthsUntilDue { get; private set; }
        public int DaysUntilDue { get; private set; }
        public string CountdownText { get; private set; } = "";
        public ObservableCollection<BabyModel> BabyProfiles { get; }
        public bool HasBabyProfiles => BabyProfiles.Count > 0;
        public bool HasNoBabyProfiles => !HasBabyProfiles;
        public string ProfileCountText => BabyProfiles.Count switch
        {
            0 => "None yet",
            1 => "1 baby",
            _ => $"{BabyProfiles.Count} babies"
        };

        //Each baby's vaccine tag, only for babies with something due; the cards look theirs up through VaccineTagConverter
        public IReadOnlyDictionary<Guid, string> VaccineTagsByBabyId { get; private set; } = new Dictionary<Guid, string>();

        public string WeeklyAppointmentText => CountAppointmentsThisWeek() switch
        {
            0 => "Nothing planned",
            1 => "1 planned",
            var count => $"{count} planned"
        };

        public string CompletedTaskText => _taskListService.Tasks.Count(task => task.IsCompleted) switch
        {
            0 => "None yet",
            1 => "1 done",
            var count => $"{count} done"
        };

        public MainPage(ITaskListService taskListService, IBabyProfileService babyProfileService, IVaccineCatalog vaccineCatalog, IVaccineService vaccineService, IApiKeyStore apiKeyStore,
            CubReplyService cubReplyService, ICubPersonalityStore personalityStore, IBabyFilterService babyFilterService)
        {
            InitializeComponent();

            _taskListService = taskListService;
            _babyProfileService = babyProfileService;
            _vaccineCatalog = vaccineCatalog;
            _vaccineService = vaccineService;
            _apiKeyStore = apiKeyStore;
            _cubReplyService = cubReplyService;
            _personalityStore = personalityStore;
            _babyFilterService = babyFilterService;
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
            RefreshBabyCards();
            HookWindowResumed();

            _ = Companion.GreetAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Companion.StopGreeting();
        }

        //The tab stays alive while the app is in the background, so a day change (or the due date passing) while away
        //is only noticed when the app comes back. The page hooks its own window, once, as soon as it has one.
        private void HookWindowResumed()
        {
            if (Window is null || ReferenceEquals(Window, _resumedWindow))
            {
                return;
            }

            if (_resumedWindow is not null)
            {
                _resumedWindow.Resumed -= OnWindowResumed;
            }

            _resumedWindow = Window;
            _resumedWindow.Resumed += OnWindowResumed;
        }

        private void OnWindowResumed(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(() =>
        {
            RefreshDashboardState();
            RefreshBabyCards();
        });

        private void OnSummaryTilesSizeChanged(object? sender, EventArgs e) => LayOutSummaryTiles();

        //Week, Tasks and Babies sit three across while every title fits its column. When one wouldn't (a narrow phone, or larger text)
        //the tiles become three full-width rows, emoji left and title with value right, instead of breaking a title mid-word. The choice
        //comes from the grid's width and the titles' natural width, never from the current layout, so it can't flip back and forth
        private void LayOutSummaryTiles()
        {
            if (SummaryTiles.Width <= 0)
            {
                return;
            }

            (Border Tile, Grid Body, Border Icon, Label Title, Label Value)[] tiles =
            [
                (WeekTile, WeekTileBody, WeekIcon, WeekTitle, WeekValue),
                (TasksTile, TasksTileBody, TasksIcon, TasksTitle, TasksValue),
                (BabiesTile, BabiesTileBody, BabiesIcon, BabiesTitle, BabiesValue)
            ];

            double columnWidth = (SummaryTiles.Width - 2 * SummaryTiles.ColumnSpacing) / 3;
            double widestTitle = tiles.Max(tile => tile.Title.Measure(double.PositiveInfinity, double.PositiveInfinity).Width);
            bool acrossFits = widestTitle <= columnWidth - tiles[0].Tile.Padding.HorizontalThickness;

            SummaryTiles.ColumnDefinitions = acrossFits ? Columns(3) : Columns(1);
            SummaryTiles.RowDefinitions = Rows(acrossFits ? 0 : 3);
            SummaryTiles.RowSpacing = acrossFits ? 0 : SummaryTiles.ColumnSpacing;

            for (int i = 0; i < tiles.Length; i++)
            {
                var (tile, body, icon, title, value) = tiles[i];

                Grid.SetColumn(tile, acrossFits ? i : 0);
                Grid.SetRow(tile, acrossFits ? 0 : i);

                body.ColumnSpacing = acrossFits ? 0 : 14;
                body.RowSpacing = acrossFits ? 8 : 2;
                body.RowDefinitions = Rows(acrossFits ? 3 : 2);

                //Across: emoji over title over value, centered. Rows: the emoji spans both lines on the left
                Grid.SetColumn(icon, 0);
                Grid.SetRow(icon, 0);
                Grid.SetColumnSpan(icon, acrossFits ? 2 : 1);
                Grid.SetRowSpan(icon, acrossFits ? 1 : 2);
                icon.VerticalOptions = LayoutOptions.Center;

                Grid.SetColumn(title, acrossFits ? 0 : 1);
                Grid.SetRow(title, acrossFits ? 1 : 0);
                Grid.SetColumnSpan(title, acrossFits ? 2 : 1);

                Grid.SetColumn(value, acrossFits ? 0 : 1);
                Grid.SetRow(value, acrossFits ? 2 : 1);
                Grid.SetColumnSpan(value, acrossFits ? 2 : 1);

                TextAlignment alignment = acrossFits ? TextAlignment.Center : TextAlignment.Start;
                title.HorizontalTextAlignment = alignment;
                value.HorizontalTextAlignment = alignment;
            }
        }

        private static ColumnDefinitionCollection Columns(int count)
        {
            var columns = new ColumnDefinitionCollection();

            for (int i = 0; i < count; i++)
            {
                columns.Add(new ColumnDefinition(GridLength.Star));
            }

            return columns;
        }

        private static RowDefinitionCollection Rows(int count)
        {
            var rows = new RowDefinitionCollection();

            for (int i = 0; i < count; i++)
            {
                rows.Add(new RowDefinition(GridLength.Auto));
            }

            return rows;
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
        private async void OnTalkToCubRequested(object? sender, EventArgs e) => await PushOnceAsync(() =>
            new ChatPage(_apiKeyStore, _cubReplyService, _personalityStore, _babyProfileService, _babyFilterService, _vaccineService, _vaccineCatalog));

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

        //BabyModel has no change notification, so raising BabyProfiles (the same instance) wouldn't redraw a card's age or tag.
        //Assigning the source again does; it replaces the binding, which only ever pointed at this collection. It resets the
        //list's scroll position, so only the moments the page is shown again call it (appearing, resuming), not every collection change.
        private void RefreshBabyCards()
        {
            BabyList.ItemsSource = null;
            BabyList.ItemsSource = BabyProfiles;
        }

        private void RefreshDashboardState()
        {
            HandleCounter(); //Recomputes against today's date: the countdown only changes when the date does
            VaccineTagsByBabyId = BuildVaccineTags();

            OnPropertyChanged(nameof(CountdownText));
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
