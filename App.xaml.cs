using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Services;

namespace BabyBuddyHelper
{
    public partial class App : Application
    {
        private readonly ITaskListService _taskListService;
        private readonly IBabyProfileService _babyProfileService;
        private readonly ITrackerDbService _trackerDbService;

        public App(ITaskListService taskListService, IBabyProfileService babyProfileService, ITrackerDbService trackerDbService)
        {
            InitializeComponent();

            _taskListService = taskListService;
            _babyProfileService = babyProfileService;
            _trackerDbService = trackerDbService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new(new AppShell());
            window.Created += OnWindowCreated;
            return window;
        }

        //Pages already exist at this point and fill in through CollectionChanged as the caches load.
        //Created can fire again if the platform re-creates the native window; the services load only once and the
        //seeder only writes into an empty store, so repeats are harmless.
        private async void OnWindowCreated(object? sender, EventArgs e)
        {
            await _babyProfileService.InitializeAsync(); //Profiles first, so any baby a task refers to already exists
            await TrackerDataSeeder.SeedIfEmptyAsync(_trackerDbService);
            await _taskListService.InitializeAsync();
        }
    }
}
