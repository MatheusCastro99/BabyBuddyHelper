using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    public class BabyProfileCrudTests : DatabaseTest
    {
        [Fact]
        public async Task AddAsync_Profile_IsCachedAndStored()
        {
            DateTime lastFeed = DateTime.Today.AddHours(8);
            DateTime lastSleep = DateTime.Today.AddHours(6);
            var baby = new BabyModel
            {
                Name = "Olivia",
                DateOfBirth = DateTime.Today.AddMonths(-3),
                WeightInLbs = 12.5,
                HeightInFt = 1.9,
                LastFeed = lastFeed,
                LastSleep = lastSleep
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);

                Assert.Same(baby, Assert.Single(services.BabyProfiles.BabyProfiles));
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Equal(baby.Id, stored.Id);
            Assert.Equal("Olivia", stored.Name);
            Assert.Equal(DateTime.Today.AddMonths(-3), stored.DateOfBirth);
            Assert.Equal(12.5, stored.WeightInLbs);
            Assert.Equal(1.9, stored.HeightInFt);
            Assert.Equal(lastFeed, stored.LastFeed);
            Assert.Equal(lastSleep, stored.LastSleep);
        }

        //The caller passes a different instance carrying the same Id, the way AddBabyPage does (ADR-007)
        [Fact]
        public async Task UpdateAsync_ExistingId_ReplacesEntryAndStoresIt()
        {
            BabyModel original = TestData.Baby("Olivia");
            var edited = new BabyModel { Id = original.Id, Name = "Olivia Rose", DateOfBirth = DateTime.Today.AddMonths(-4), WeightInLbs = 13 };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(original);

                await services.BabyProfiles.UpdateAsync(edited);

                Assert.Same(edited, Assert.Single(services.BabyProfiles.BabyProfiles));
            }

            using TestServices restarted = await StartServicesAsync();
            BabyModel stored = Assert.Single(restarted.BabyProfiles.BabyProfiles);

            Assert.Equal(original.Id, stored.Id);
            Assert.Equal("Olivia Rose", stored.Name);
            Assert.Equal(DateTime.Today.AddMonths(-4), stored.DateOfBirth);
            Assert.Equal(13, stored.WeightInLbs);
        }

        [Fact]
        public async Task UpdateAsync_UnknownId_ChangesNothing()
        {
            BabyModel existing = TestData.Baby("Olivia");

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(existing);

                await services.BabyProfiles.UpdateAsync(TestData.Baby("Never added"));

                Assert.Same(existing, Assert.Single(services.BabyProfiles.BabyProfiles));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal("Olivia", Assert.Single(restarted.BabyProfiles.BabyProfiles).Name);
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_RemovesOnlyThatProfile()
        {
            BabyModel removed = TestData.Baby("Olivia");
            BabyModel kept = TestData.Baby("Noah");

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(removed);
                await services.BabyProfiles.AddAsync(kept);

                await services.BabyProfiles.RemoveAsync(removed.Id);

                Assert.Same(kept, Assert.Single(services.BabyProfiles.BabyProfiles));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(kept.Id, Assert.Single(restarted.BabyProfiles.BabyProfiles).Id);
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ChangesNothing()
        {
            BabyModel existing = TestData.Baby("Olivia");

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(existing);

                await services.BabyProfiles.RemoveAsync(Guid.NewGuid());

                Assert.Same(existing, Assert.Single(services.BabyProfiles.BabyProfiles));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(existing.Id, Assert.Single(restarted.BabyProfiles.BabyProfiles).Id);
        }
    }
}
