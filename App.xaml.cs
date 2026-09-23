using BabyBuddyHelper.Exceptions;
using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Services;

namespace BabyBuddyHelper
{
    public partial class App : Application
    {
        private readonly ITaskListService _taskListService;
        private readonly IBabyProfileService _babyProfileService;
        private Task? _startupTask;

        public App(ITaskListService taskListService, IBabyProfileService babyProfileService)
        {
            InitializeComponent();

            _taskListService = taskListService;
            _babyProfileService = babyProfileService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new(new AppShell());
            window.Created += OnWindowCreated;
            return window;
        }

        //Pages already exist at this point and fill in through CollectionChanged as the caches load.
        //Created can fire again if the platform re-creates the native window, possibly while an earlier run is still
        //awaiting. Every invocation shares one startup run, so the seeder's check-then-insert can never run twice,
        //and a failed load shows its alerts only once.
        private async void OnWindowCreated(object? sender, EventArgs e)
        {
            if (sender is not Window { Page: { } page })
            {
                return; //Only subscribed on windows built in CreateWindow, which always carry the shell
            }

            if (_startupTask is null || _startupTask.IsFaulted || _startupTask.IsCanceled)
            {
                _startupTask = RunStartupAsync(page);
            }

            await _startupTask;
        }

        private async Task RunStartupAsync(Page page)
        {
            while (true)
            {
                try
                {
                    //Profiles first, so any baby a task refers to already exists. On a retry, whichever load already
                    //succeeded returns its finished result instead of loading again, so nothing is duplicated.
                    await _babyProfileService.InitializeAsync();
                    await _taskListService.InitializeAsync();
                    break;
                }
                catch (DbCommunicationException)
                {
                    if (await AlertService.ShowStartupLoadFailedAsync(page))
                    {
                        continue; //"Try again"
                    }

                    //"Continue anyway": seeding is skipped, since an empty cache here doesn't mean an empty database
                    await AlertService.ShowRunningWithoutDataAsync(page);
                    return;
                }
            }

            //Seeds through the cache, so TaskListService stays the only task writer (ADR-001). Deliberately not caught:
            //seeding only runs in Debug builds, so a failed seed should crash loudly rather than be reported to a user.
            await TrackerDataSeeder.SeedIfEmptyAsync(_taskListService);
        }
    }
}
