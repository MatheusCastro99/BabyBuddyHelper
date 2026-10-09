namespace BabyBuddyHelper.Tests.Infrastructure
{
    //Base for tests that need stored data. xUnit creates a new instance of the test class for every test, so each test
    //gets its own empty database and no test can see another's records.
    public abstract class DatabaseTest : IDisposable
    {
        protected TestDatabase Database { get; } = new();

        //Call it again within a test to stand in for an app restart
        protected Task<TestServices> StartServicesAsync() => TestServices.StartAsync(Database);

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
