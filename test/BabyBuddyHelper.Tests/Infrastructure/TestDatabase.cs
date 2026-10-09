using BabyBuddyHelper.Core.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //A private SQLite database held in memory, one per test. It exists only while its connection stays open, so nothing is
    //written to disk and the app's babybuddy.db3 is never touched. Disposing it drops the database.
    public sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public IDbContextFactory<TrackerContext> ContextFactory { get; }

        public TestDatabase()
        {
            //Foreign keys are asked for explicitly: the connection is opened here rather than by EF Core, and the delete rules
            //in TrackerContext (ADR-011) only run when they are on.
            _connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
            _connection.Open();

            DbContextOptions<TrackerContext> options = new DbContextOptionsBuilder<TrackerContext>()
                .UseSqlite(_connection)
                .Options;

            ContextFactory = new TestContextFactory(options);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        //Every context shares the one open connection, so each sees the same in-memory database. Same shape as the app:
        //EfTrackerDbService still gets a new short-lived context per operation (ADR-002).
        private sealed class TestContextFactory : IDbContextFactory<TrackerContext>
        {
            private readonly DbContextOptions<TrackerContext> _options;

            public TestContextFactory(DbContextOptions<TrackerContext> options)
            {
                _options = options;
            }

            public TrackerContext CreateDbContext() => new TrackerContext(_options);
        }
    }
}
