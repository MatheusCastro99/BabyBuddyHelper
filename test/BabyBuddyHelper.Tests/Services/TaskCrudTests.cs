using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    public class TaskCrudTests : DatabaseTest
    {
        [Fact]
        public async Task AddAsync_Task_IsCachedAndStored()
        {
            TaskModel task = TestData.Task("Buy crib", priority: 2);

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(task);

                Assert.Same(task, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            TaskModel stored = Assert.Single(restarted.Tasks.Tasks);

            Assert.Equal(task.Id, stored.Id);
            Assert.IsType<TaskModel>(stored);
            Assert.Equal("Buy crib", stored.TaskName);
            Assert.Equal("Check the safety rating", stored.TaskDescription);
            Assert.Equal(2, stored.TaskPriority);
            Assert.False(stored.IsCompleted);
            Assert.Null(stored.AssociatedBabyId);
        }

        [Fact]
        public async Task AddAsync_Appointment_KeepsTypeAndDetailsAfterRestart()
        {
            AppointmentModel appointment = TestData.Appointment("Pediatrician visit");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(appointment);
            }

            using TestServices restarted = await StartServicesAsync();
            AppointmentModel stored = Assert.IsType<AppointmentModel>(Assert.Single(restarted.Tasks.Tasks));

            Assert.Equal(appointment.Id, stored.Id);
            Assert.Equal("Pediatrician visit", stored.TaskName);
            Assert.Equal("Riverside Clinic", stored.AppointmentLocation);
            Assert.Equal(DateTime.Today.AddDays(7), stored.AppointmentDate);
            Assert.Equal(new TimeSpan(9, 30, 0), stored.AppointmentStartTime);
            Assert.Equal(new TimeSpan(10, 15, 0), stored.AppointmentEndTime);
            Assert.Same(stored, Assert.Single(restarted.Tasks.GetAppointments()));
        }

        //The caller passes a different instance carrying the same Id, the way AddTaskPage does (ADR-007)
        [Fact]
        public async Task UpdateAsync_ExistingId_ReplacesEntryAndStoresIt()
        {
            TaskModel original = TestData.Task("Buy crib", priority: 2);
            var edited = new TaskModel(5, "Buy bassinet", "Fits next to the bed") { Id = original.Id, IsCompleted = true };

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(original);

                await services.Tasks.UpdateAsync(edited);

                Assert.Same(edited, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            TaskModel stored = Assert.Single(restarted.Tasks.Tasks);

            Assert.Equal(original.Id, stored.Id);
            Assert.Equal("Buy bassinet", stored.TaskName);
            Assert.Equal("Fits next to the bed", stored.TaskDescription);
            Assert.Equal(5, stored.TaskPriority);
            Assert.True(stored.IsCompleted);
        }

        [Fact]
        public async Task UpdateAsync_UnknownId_ChangesNothing()
        {
            TaskModel existing = TestData.Task("Buy crib");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(existing);

                await services.Tasks.UpdateAsync(TestData.Task("Never added"));

                Assert.Same(existing, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal("Buy crib", Assert.Single(restarted.Tasks.Tasks).TaskName);
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_RemovesOnlyThatTask()
        {
            TaskModel removed = TestData.Task("Buy crib");
            TaskModel kept = TestData.Task("Pack hospital bag");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(removed);
                await services.Tasks.AddAsync(kept);

                await services.Tasks.RemoveAsync(removed.Id);

                Assert.Same(kept, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(kept.Id, Assert.Single(restarted.Tasks.Tasks).Id);
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ChangesNothing()
        {
            TaskModel existing = TestData.Task("Buy crib");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(existing);

                await services.Tasks.RemoveAsync(Guid.NewGuid());

                Assert.Same(existing, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(existing.Id, Assert.Single(restarted.Tasks.Tasks).Id);
        }

        //The cached entry is changed in place, not replaced, so the checklist doesn't rebuild on every tick
        [Fact]
        public async Task SetCompletionAsync_ExistingId_StoresStateAndKeepsCachedInstance()
        {
            TaskModel task = TestData.Task("Buy crib");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(task);

                await services.Tasks.SetCompletionAsync(task.Id, true);

                Assert.Same(task, Assert.Single(services.Tasks.Tasks));
                Assert.True(task.IsCompleted);
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.True(Assert.Single(restarted.Tasks.Tasks).IsCompleted);
        }

        [Fact]
        public async Task SetCompletionAsync_BackToPending_IsStored()
        {
            TaskModel task = TestData.Task("Buy crib");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(task);
                await services.Tasks.SetCompletionAsync(task.Id, true);

                await services.Tasks.SetCompletionAsync(task.Id, false);
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.False(Assert.Single(restarted.Tasks.Tasks).IsCompleted);
        }
    }
}
