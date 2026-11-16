using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.TaskList;
using BabyBuddyHelper.Core.Persistence;
using BabyBuddyHelper.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BabyBuddyHelper.Tests.Services
{
    //The schema is built and upgraded by EF Core migrations (ADR-002). These tests sit at the Services root because they cover
    //every feature's tables. A failure here after a model change usually means the migration for it was not generated.
    public class MigrationTests : DatabaseTest
    {
        [Fact]
        public async Task FirstServiceCall_OnEmptyDatabase_AppliesEveryMigration()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;

            using TestServices services = await StartServicesAsync();

            await using TrackerContext db = await Database.ContextFactory.CreateDbContextAsync(ct);

            string[] allMigrations = db.Database.GetMigrations().ToArray();
            Assert.Contains(allMigrations, x => x.EndsWith("_InitialCreate"));
            Assert.Equal(allMigrations, await db.Database.GetAppliedMigrationsAsync(ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
        }

        [Fact]
        public async Task Model_MatchesTheMigrationSnapshot()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;

            await using TrackerContext db = await Database.ContextFactory.CreateDbContextAsync(ct);

            //True means TrackerContext or a persisted model changed with no migration generated for it
            Assert.False(db.Database.HasPendingModelChanges());
        }

        [Fact]
        public async Task Migrate_FromInitialCreate_KeepsExistingRows()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;

            Guid babyId = Guid.NewGuid();
            Guid taskId = Guid.NewGuid();

            await using (TrackerContext db = await Database.ContextFactory.CreateDbContextAsync(ct))
            {
                //Bring the database to the first schema, the way an install of the first release would have it
                string initialCreate = db.Database.GetMigrations().First();
                await db.GetService<IMigrator>().MigrateAsync(initialCreate, cancellationToken: ct);

                //Raw SQL, so the rows don't depend on the current model: this is data written by an older build
                await db.Database.ExecuteSqlAsync($"""
                    INSERT INTO BabyProfiles (Id, Name, DateOfBirth, WeightInLbs, HeightInFt, LastFeed, LastSleep)
                    VALUES ({babyId}, 'Olivia', '2026-08-01 00:00:00', 12.5, 2.1, '2026-11-01 08:00:00', '2026-11-01 07:00:00')
                    """, ct);
                await db.Database.ExecuteSqlAsync($"""
                    INSERT INTO Tasks (Id, AssociatedBabyId, TaskPriority, TaskName, TaskDescription, IsCompleted, TaskType)
                    VALUES ({taskId}, {babyId}, 2, 'Buy crib', 'Check the safety rating', 1, 'Task')
                    """, ct);
            }

            //The app's first call migrates to the latest schema. Every later migration has to carry these rows over.
            using TestServices services = await StartServicesAsync();

            BabyModel baby = Assert.Single(services.BabyProfiles.BabyProfiles);
            Assert.Equal(babyId, baby.Id);
            Assert.Equal("Olivia", baby.Name);
            Assert.Equal(12.5, baby.WeightInLbs);

            TaskModel task = Assert.Single(services.Tasks.Tasks);
            Assert.Equal(taskId, task.Id);
            Assert.Equal(babyId, task.AssociatedBabyId);
            Assert.Equal("Buy crib", task.TaskName);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Migrate_AlreadyUpToDate_ChangesNothing()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;

            BabyModel baby = TestData.Baby();

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
            }

            await using (TrackerContext db = await Database.ContextFactory.CreateDbContextAsync(ct))
            {
                await db.Database.MigrateAsync(ct);

                Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal(baby.Id, Assert.Single(restarted.BabyProfiles.BabyProfiles).Id);
        }
    }
}
