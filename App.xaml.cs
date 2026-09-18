using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Services;

namespace BabyBuddyHelper
{
    public partial class App : Application
    {
        private readonly ITaskListService _taskListService;
        private readonly IBabyProfileService _babyProfileService;
        private readonly ITrackerDbService _trackerDbService;
        private Task? _startupTask;

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
        //Created can fire again if the platform re-creates the native window, possibly while an earlier run is still
        //awaiting. Every invocation shares one startup run, so the seeder's check-then-insert can never run twice.
        //Startup error handling (surfacing or recovering from a failed load) is deferred to the cache work in #26.
        private async void OnWindowCreated(object? sender, EventArgs e)
        {
            if (_startupTask is null || _startupTask.IsFaulted || _startupTask.IsCanceled)
            {
                _startupTask = RunStartupAsync();
            }

            await _startupTask;
        }

        private async Task RunStartupAsync()
        {
            await _babyProfileService.InitializeAsync(); //Profiles first, so any baby a task refers to already exists
            await TrackerDataSeeder.SeedIfEmptyAsync(_trackerDbService);
            await _taskListService.InitializeAsync();
        }
    }
}
