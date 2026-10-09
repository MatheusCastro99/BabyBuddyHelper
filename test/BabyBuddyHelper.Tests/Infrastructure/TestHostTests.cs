using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //Checks on the test host itself. If one of these fails, the other test classes can't be trusted.
    public class TestHostTests : IDisposable
    {
        private readonly TestDatabase _database = new();

        public void Dispose()
        {
            _database.Dispose();
        }

        [Fact]
        public async Task StartAsync_NewDatabase_StartsEmpty()
        {
            using TestServices services = await TestServices.StartAsync(_database);

            Assert.Empty(services.BabyProfiles.BabyProfiles);
            Assert.Empty(services.Tasks.Tasks);
            Assert.Empty(services.Vaccines.VaccinationRecords);
        }

        [Fact]
        public async Task StartAsync_SecondSetOnSameDatabase_LoadsWhatTheFirstStored()
        {
            var task = new TaskModel(1, "Buy crib", "Check the safety rating");

            using (TestServices services = await TestServices.StartAsync(_database))
            {
                await services.Tasks.AddAsync(task);
            }

            using TestServices restarted = await TestServices.StartAsync(_database);

            TaskModel stored = Assert.Single(restarted.Tasks.Tasks);
            Assert.Equal(task.Id, stored.Id);
            Assert.NotSame(task, stored); //Read back from the database, not handed over in memory
        }

        [Fact]
        public async Task StartAsync_SeparateDatabases_ShareNothing()
        {
            using var otherDatabase = new TestDatabase();
            using TestServices services = await TestServices.StartAsync(_database);
            using TestServices otherServices = await TestServices.StartAsync(otherDatabase);

            await services.Tasks.AddAsync(new TaskModel(1, "Buy crib", "Check the safety rating"));

            using TestServices otherRestarted = await TestServices.StartAsync(otherDatabase);
            Assert.Empty(otherServices.Tasks.Tasks);
            Assert.Empty(otherRestarted.Tasks.Tasks);
        }

        //The delete rules tested elsewhere (ADR-011) depend on SQLite enforcing foreign keys, which is off unless asked for
        [Fact]
        public async Task Database_RecordForMissingBaby_IsRejected()
        {
            using TestServices services = await TestServices.StartAsync(_database);
            var record = new VaccinationRecordModel { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid() };

            await Assert.ThrowsAsync<DbCommunicationException>(() => services.Vaccines.AddAsync(record));

            Assert.Empty(services.Vaccines.VaccinationRecords);
        }
    }
}
