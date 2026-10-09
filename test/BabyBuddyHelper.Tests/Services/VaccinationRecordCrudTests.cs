using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    public class VaccinationRecordCrudTests : DatabaseTest
    {
        [Fact]
        public async Task AddAsync_Record_IsCachedAndStored()
        {
            BabyModel baby = TestData.Baby();
            var record = new VaccinationRecordModel
            {
                BabyId = baby.Id,
                VaccineId = Guid.NewGuid(),
                TotalDoses = 3,
                CompletedDoses = 1,
                LastAdministered = DateTime.Today.AddDays(-30),
                NextDose = DateTime.Today.AddDays(30)
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(record);

                Assert.Same(record, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            VaccinationRecordModel stored = Assert.Single(restarted.Vaccines.VaccinationRecords);

            Assert.Equal(record.Id, stored.Id);
            Assert.Equal(baby.Id, stored.BabyId);
            Assert.Equal(record.VaccineId, stored.VaccineId);
            Assert.Equal(3, stored.TotalDoses);
            Assert.Equal(1, stored.CompletedDoses);
            Assert.Equal(DateTime.Today.AddDays(-30), stored.LastAdministered);
            Assert.Equal(DateTime.Today.AddDays(30), stored.NextDose);
        }

        //A seasonal vaccine has no dose total and may have no dates yet
        [Fact]
        public async Task AddAsync_RecordWithEmptyOptionalFields_StoresThemEmpty()
        {
            BabyModel baby = TestData.Baby();
            var record = new VaccinationRecordModel { BabyId = baby.Id, VaccineId = Guid.NewGuid() };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(record);
            }

            using TestServices restarted = await StartServicesAsync();
            VaccinationRecordModel stored = Assert.Single(restarted.Vaccines.VaccinationRecords);

            Assert.Null(stored.TotalDoses);
            Assert.Equal(0, stored.CompletedDoses);
            Assert.Null(stored.LastAdministered);
            Assert.Null(stored.NextDose);
        }

        //The caller passes a different instance carrying the same Id, the way AddVaccineRecordPage does (ADR-007)
        [Fact]
        public async Task UpdateAsync_ExistingId_ReplacesEntryAndStoresIt()
        {
            BabyModel baby = TestData.Baby();
            VaccinationRecordModel original = TestData.Record(baby.Id);
            var edited = new VaccinationRecordModel
            {
                Id = original.Id,
                BabyId = original.BabyId,
                VaccineId = original.VaccineId,
                TotalDoses = 4,
                CompletedDoses = 2,
                LastAdministered = DateTime.Today,
                NextDose = DateTime.Today.AddDays(60)
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(original);

                await services.Vaccines.UpdateAsync(edited);

                Assert.Same(edited, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            VaccinationRecordModel stored = Assert.Single(restarted.Vaccines.VaccinationRecords);

            Assert.Equal(original.Id, stored.Id);
            Assert.Equal(4, stored.TotalDoses);
            Assert.Equal(2, stored.CompletedDoses);
            Assert.Equal(DateTime.Today, stored.LastAdministered);
            Assert.Equal(DateTime.Today.AddDays(60), stored.NextDose);
        }

        [Fact]
        public async Task UpdateAsync_UnknownId_ChangesNothing()
        {
            BabyModel baby = TestData.Baby();
            VaccinationRecordModel existing = TestData.Record(baby.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(existing);

                await services.Vaccines.UpdateAsync(TestData.Record(baby.Id));

                Assert.Same(existing, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(existing.Id, Assert.Single(restarted.Vaccines.VaccinationRecords).Id);
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_RemovesOnlyThatRecord()
        {
            BabyModel baby = TestData.Baby();
            VaccinationRecordModel removed = TestData.Record(baby.Id);
            VaccinationRecordModel kept = TestData.Record(baby.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(removed);
                await services.Vaccines.AddAsync(kept);

                await services.Vaccines.RemoveAsync(removed.Id);

                Assert.Same(kept, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(kept.Id, Assert.Single(restarted.Vaccines.VaccinationRecords).Id);
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ChangesNothing()
        {
            BabyModel baby = TestData.Baby();
            VaccinationRecordModel existing = TestData.Record(baby.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(existing);

                await services.Vaccines.RemoveAsync(Guid.NewGuid());

                Assert.Same(existing, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(existing.Id, Assert.Single(restarted.Vaccines.VaccinationRecords).Id);
        }

        [Fact]
        public async Task GetRecordsForBaby_TwoBabies_ReturnsOnlyThatBabysRecords()
        {
            BabyModel olivia = TestData.Baby("Olivia");
            BabyModel noah = TestData.Baby("Noah");
            VaccinationRecordModel oliviaRecord = TestData.Record(olivia.Id);

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(olivia);
            await services.BabyProfiles.AddAsync(noah);
            await services.Vaccines.AddAsync(oliviaRecord);
            await services.Vaccines.AddAsync(TestData.Record(noah.Id));

            Assert.Same(oliviaRecord, Assert.Single(services.Vaccines.GetRecordsForBaby(olivia.Id)));
        }

        //At most one record per baby per vaccine: the database's unique index refuses the second (ADR-011)
        [Fact]
        public async Task AddAsync_SecondRecordForSameBabyAndVaccine_ThrowsAndLeavesTheFirst()
        {
            BabyModel baby = TestData.Baby();
            Guid vaccineId = Guid.NewGuid();
            VaccinationRecordModel first = TestData.Record(baby.Id, vaccineId);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Vaccines.AddAsync(first);

                await Assert.ThrowsAsync<DbCommunicationException>(() => services.Vaccines.AddAsync(TestData.Record(baby.Id, vaccineId)));

                Assert.Same(first, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(first.Id, Assert.Single(restarted.Vaccines.VaccinationRecords).Id);
        }

        [Fact]
        public async Task AddAsync_SameVaccineForAnotherBaby_IsStored()
        {
            BabyModel olivia = TestData.Baby("Olivia");
            BabyModel noah = TestData.Baby("Noah");
            Guid vaccineId = Guid.NewGuid();

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(olivia);
                await services.BabyProfiles.AddAsync(noah);
                await services.Vaccines.AddAsync(TestData.Record(olivia.Id, vaccineId));

                await services.Vaccines.AddAsync(TestData.Record(noah.Id, vaccineId));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(2, restarted.Vaccines.VaccinationRecords.Count);
        }
    }
}
