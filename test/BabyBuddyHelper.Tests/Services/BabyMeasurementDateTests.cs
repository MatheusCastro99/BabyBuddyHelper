using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Core.Persistence;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //BabyProfileService dates each measurement: a changed weight or height is dated today, an unchanged one keeps its
    //date, and 0 (not measured) has none. The dates are worked out against what is stored, never taken from the caller.
    public class BabyMeasurementDateTests : DatabaseTest
    {
        private static readonly DateTime LastWeek = DateTime.Today.AddDays(-7);
        private static readonly DateTime LastMonth = DateTime.Today.AddMonths(-1);

        [Fact]
        public async Task AddAsync_WithMeasurements_DatesThemToday()
        {
            var baby = new BabyModel { Name = "Olivia", WeightInLbs = 12.5, HeightInFt = 1.9 };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Equal(DateTime.Today, stored.WeightUpdatedOn);
            Assert.Equal(DateTime.Today, stored.HeightUpdatedOn);
        }

        [Fact]
        public async Task AddAsync_WithoutMeasurements_LeavesTheDatesEmpty()
        {
            //The dates passed in are ignored: nothing was measured, so there is nothing to date
            var baby = new BabyModel { Name = "Olivia", WeightUpdatedOn = LastWeek, HeightUpdatedOn = LastWeek };

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(baby);

            BabyModel cached = Assert.Single(services.BabyProfiles.BabyProfiles);
            Assert.Null(cached.WeightUpdatedOn);
            Assert.Null(cached.HeightUpdatedOn);
        }

        [Fact]
        public async Task AddAsync_DatesFromTheCaller_AreIgnored()
        {
            var baby = new BabyModel { Name = "Olivia", WeightInLbs = 12.5, HeightInFt = 1.9, WeightUpdatedOn = LastWeek, HeightUpdatedOn = LastMonth };

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(baby);

            BabyModel cached = Assert.Single(services.BabyProfiles.BabyProfiles);
            Assert.Equal(DateTime.Today, cached.WeightUpdatedOn);
            Assert.Equal(DateTime.Today, cached.HeightUpdatedOn);
        }

        [Fact]
        public async Task UpdateAsync_ChangedWeight_DatesItTodayAndKeepsTheHeightDate()
        {
            BabyModel existing = await StoreMeasuredBabyAsync();
            var edited = new BabyModel { Id = existing.Id, Name = "Olivia", WeightInLbs = 13.1, HeightInFt = 1.9 };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.UpdateAsync(edited);
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Equal(13.1, stored.WeightInLbs);
            Assert.Equal(DateTime.Today, stored.WeightUpdatedOn);
            Assert.Equal(LastMonth, stored.HeightUpdatedOn);
        }

        [Fact]
        public async Task UpdateAsync_UnchangedMeasurements_KeepTheirDates()
        {
            BabyModel existing = await StoreMeasuredBabyAsync();

            //Only the name changes. The dates sent along are wrong on purpose: the stored ones must win.
            var edited = new BabyModel { Id = existing.Id, Name = "Olivia Rose", WeightInLbs = 12.5, HeightInFt = 1.9, WeightUpdatedOn = DateTime.Today, HeightUpdatedOn = null };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.UpdateAsync(edited);
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Equal("Olivia Rose", stored.Name);
            Assert.Equal(LastWeek, stored.WeightUpdatedOn);
            Assert.Equal(LastMonth, stored.HeightUpdatedOn);
        }

        //AddBabyPage shows two decimals, so a value that only differs beyond them is the same measurement
        [Fact]
        public async Task UpdateAsync_SameWeightAfterRounding_KeepsItsDate()
        {
            BabyModel existing = await StoreMeasuredBabyAsync();
            var edited = new BabyModel { Id = existing.Id, Name = "Olivia", WeightInLbs = 12.5049, HeightInFt = 1.9 };

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.UpdateAsync(edited);

            BabyModel cached = Assert.Single(services.BabyProfiles.BabyProfiles);
            Assert.Equal(12.5, cached.WeightInLbs);
            Assert.Equal(LastWeek, cached.WeightUpdatedOn);
        }

        [Fact]
        public async Task UpdateAsync_MeasurementClearedToZero_LosesItsDate()
        {
            BabyModel existing = await StoreMeasuredBabyAsync();
            var edited = new BabyModel { Id = existing.Id, Name = "Olivia", WeightInLbs = 0, HeightInFt = 1.9 };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.UpdateAsync(edited);
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Null(stored.WeightUpdatedOn);
            Assert.Equal(LastMonth, stored.HeightUpdatedOn);
        }

        //Written straight to the database, as data left by an earlier session: BabyProfileService would date it today
        private async Task<BabyModel> StoreMeasuredBabyAsync()
        {
            var baby = new BabyModel
            {
                Name = "Olivia",
                WeightInLbs = 12.5,
                HeightInFt = 1.9,
                WeightUpdatedOn = LastWeek,
                HeightUpdatedOn = LastMonth
            };

            await new EfTrackerDbService(Database.ContextFactory).AddBabyProfileAsync(baby);
            return baby;
        }
    }
}
