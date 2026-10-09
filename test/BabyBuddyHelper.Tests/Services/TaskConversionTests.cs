using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //Tasks and appointments share one table. Converting one into the other changes the row's type, which the database
    //service does as a delete and re-insert under the same Id (see EfTrackerDbService.UpdateTaskAsync).
    public class TaskConversionTests : DatabaseTest
    {
        [Fact]
        public async Task UpdateAsync_TaskToAppointment_KeepsIdAndChangesType()
        {
            TaskModel task = TestData.Task("Pediatrician visit");
            var appointment = new AppointmentModel("Riverside Clinic", DateTime.Today.AddDays(3), new TimeSpan(14, 0, 0), new TimeSpan(14, 45, 0), 4, "Pediatrician visit", "Bring the vaccine card")
            {
                Id = task.Id
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(task);

                await services.Tasks.UpdateAsync(appointment);

                Assert.Same(appointment, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            AppointmentModel stored = Assert.IsType<AppointmentModel>(Assert.Single(restarted.Tasks.Tasks));

            Assert.Equal(task.Id, stored.Id);
            Assert.Equal("Riverside Clinic", stored.AppointmentLocation);
            Assert.Equal(DateTime.Today.AddDays(3), stored.AppointmentDate);
            Assert.Equal(new TimeSpan(14, 0, 0), stored.AppointmentStartTime);
            Assert.Equal(new TimeSpan(14, 45, 0), stored.AppointmentEndTime);
            Assert.Equal("Bring the vaccine card", stored.TaskDescription);
            Assert.Equal(4, stored.TaskPriority);
        }

        [Fact]
        public async Task UpdateAsync_AppointmentToTask_KeepsIdAndChangesType()
        {
            AppointmentModel appointment = TestData.Appointment("Pediatrician visit");
            var task = new TaskModel(1, "Call the clinic", "Ask about a new date") { Id = appointment.Id };

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(appointment);

                await services.Tasks.UpdateAsync(task);

                Assert.Same(task, Assert.Single(services.Tasks.Tasks));
                Assert.Empty(services.Tasks.GetAppointments());
            }

            using TestServices restarted = await StartServicesAsync();
            TaskModel stored = Assert.Single(restarted.Tasks.Tasks);

            Assert.IsType<TaskModel>(stored);
            Assert.Equal(appointment.Id, stored.Id);
            Assert.Equal("Call the clinic", stored.TaskName);
            Assert.Empty(restarted.Tasks.GetAppointments());
        }

        [Fact]
        public async Task UpdateAsync_ConversionOfLinkedTask_KeepsBabyAndCompletion()
        {
            BabyModel baby = TestData.Baby();
            TaskModel task = TestData.Task("Pediatrician visit", babyId: baby.Id);
            var converted = new AppointmentModel("Riverside Clinic", DateTime.Today.AddDays(3), new TimeSpan(14, 0, 0), new TimeSpan(14, 45, 0), 3, "Pediatrician visit", "Two-month checkup")
            {
                Id = task.Id,
                AssociatedBabyId = baby.Id,
                IsCompleted = true
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(baby);
                await services.Tasks.AddAsync(task);

                await services.Tasks.UpdateAsync(converted);
            }

            using TestServices restarted = await StartServicesAsync();
            TaskModel stored = Assert.Single(restarted.Tasks.Tasks);

            Assert.IsType<AppointmentModel>(stored);
            Assert.Equal(baby.Id, stored.AssociatedBabyId);
            Assert.True(stored.IsCompleted);
        }

        //Other rows must survive the delete and re-insert untouched
        [Fact]
        public async Task UpdateAsync_Conversion_LeavesOtherTasksAlone()
        {
            TaskModel converting = TestData.Task("Pediatrician visit");
            TaskModel other = TestData.Task("Buy crib");
            var appointment = new AppointmentModel("Riverside Clinic", DateTime.Today.AddDays(3), new TimeSpan(14, 0, 0), new TimeSpan(14, 45, 0), 2, "Pediatrician visit", "")
            {
                Id = converting.Id
            };

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(converting);
                await services.Tasks.AddAsync(other);

                await services.Tasks.UpdateAsync(appointment);
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal(2, restarted.Tasks.Tasks.Count);
            TaskModel storedOther = Assert.Single(restarted.Tasks.Tasks, x => x.Id == other.Id);
            Assert.IsType<TaskModel>(storedOther);
            Assert.Equal("Buy crib", storedOther.TaskName);
        }
    }
}
