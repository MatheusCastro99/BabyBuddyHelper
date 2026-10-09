using BabyBuddyHelper.Core.Persistence;
using BabyBuddyHelper.Core.Services;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //The app's real cache services on top of the real EfTrackerDbService, wired the way MauiProgram wires them (ADR-014).
    //Starting a second set on the same TestDatabase stands in for an app restart: its caches are filled from what was stored.
    public sealed class TestServices : IDisposable
    {
        public BabyProfileService BabyProfiles { get; }
        public TaskListService Tasks { get; }
        public VaccineService Vaccines { get; }

        private TestServices(TestDatabase database)
        {
            var trackerDbService = new EfTrackerDbService(database.ContextFactory);

            BabyProfiles = new BabyProfileService(trackerDbService);
            Tasks = new TaskListService(BabyProfiles, trackerDbService);
            Vaccines = new VaccineService(BabyProfiles, trackerDbService);
        }

        //Loads in the app's startup order (see App.RunStartupAsync): profiles first, then tasks, then vaccination records
        public static async Task<TestServices> StartAsync(TestDatabase database)
        {
            var services = new TestServices(database);

            await services.BabyProfiles.InitializeAsync();
            await services.Tasks.InitializeAsync();
            await services.Vaccines.InitializeAsync();

            return services;
        }

        public void Dispose()
        {
            Tasks.Dispose();
            Vaccines.Dispose();
        }
    }
}
