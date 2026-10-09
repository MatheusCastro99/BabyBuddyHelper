using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //Removing a baby is decided by the database (ADR-011): its tasks stay but lose the link, and its vaccination records
    //are deleted. The cache services mirror that result, so each test checks the cache and then the stored data.
    public class BabyRemovalTests : DatabaseTest
    {
        [Fact]
        public async Task RemoveAsync_BabyWithTasks_KeepsTheTasksAndClearsTheirLink()
        {
            BabyModel baby = TestData.Baby("Olivia");
            TaskModel task = TestData.Task("Buy crib", babyId: baby.Id);
            AppointmentModel appointment = TestData.Appointment("Pediatrician visit", babyId: baby.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Tasks.AddAsync(task);
                await services.Tasks.AddAsync(appointment);

                await services.BabyProfiles.RemoveAsync(baby.Id);

                Assert.Equal(2, services.Tasks.Tasks.Count);
                Assert.All(services.Tasks.Tasks, x => Assert.Null(x.AssociatedBabyId));

                //The cache swaps each entry for a copy without the link; the copy of the appointment must still be one
                AppointmentModel cachedAppointment = Assert.IsType<AppointmentModel>(Assert.Single(services.Tasks.Tasks, x => x.Id == appointment.Id));
                Assert.Equal(appointment.AppointmentDate, cachedAppointment.AppointmentDate);
                Assert.Equal(appointment.AppointmentLocation, cachedAppointment.AppointmentLocation);
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal(2, restarted.Tasks.Tasks.Count);
            Assert.All(restarted.Tasks.Tasks, x => Assert.Null(x.AssociatedBabyId));
            Assert.IsType<TaskModel>(Assert.Single(restarted.Tasks.Tasks, x => x.Id == task.Id));
            Assert.IsType<AppointmentModel>(Assert.Single(restarted.Tasks.Tasks, x => x.Id == appointment.Id));
        }

        [Fact]
        public async Task RemoveAsync_BabyWithTasks_LeavesAnotherBabysTasksLinked()
        {
            BabyModel removed = TestData.Baby("Olivia");
            BabyModel kept = TestData.Baby("Noah");
            TaskModel keptTask = TestData.Task("Buy second crib", babyId: kept.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(removed);
                await services.BabyProfiles.AddAsync(kept);
                await services.Tasks.AddAsync(TestData.Task("Buy crib", babyId: removed.Id));
                await services.Tasks.AddAsync(keptTask);

                await services.BabyProfiles.RemoveAsync(removed.Id);

                Assert.Equal(kept.Id, Assert.Single(services.Tasks.Tasks, x => x.Id == keptTask.Id).AssociatedBabyId);
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(kept.Id, Assert.Single(restarted.Tasks.Tasks, x => x.Id == keptTask.Id).AssociatedBabyId);
        }

        [Fact]
        public async Task RemoveAsync_BabyWithRecords_DeletesOnlyItsRecords()
        {
            BabyModel removed = TestData.Baby("Olivia");
            BabyModel kept = TestData.Baby("Noah");
            Guid vaccineId = Guid.NewGuid();
            VaccinationRecordModel keptRecord = TestData.Record(kept.Id, vaccineId);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(removed);
                await services.BabyProfiles.AddAsync(kept);
                await services.Vaccines.AddAsync(TestData.Record(removed.Id, vaccineId));
                await services.Vaccines.AddAsync(TestData.Record(removed.Id));
                await services.Vaccines.AddAsync(keptRecord);

                await services.BabyProfiles.RemoveAsync(removed.Id);

                Assert.Same(keptRecord, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();
            VaccinationRecordModel stored = Assert.Single(restarted.Vaccines.VaccinationRecords);

            Assert.Equal(keptRecord.Id, stored.Id);
            Assert.Equal(kept.Id, stored.BabyId);
        }
    }
}
