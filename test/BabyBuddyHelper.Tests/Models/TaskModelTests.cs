using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Tests.Models
{
    public class TaskModelTests
    {
        [Fact]
        public void Clone_Task_KeepsIdAndFields()
        {
            var task = new TaskModel(2, "Buy crib", "Check the safety rating") { AssociatedBabyId = Guid.NewGuid(), IsCompleted = true };

            TaskModel clone = task.Clone();

            Assert.NotSame(task, clone);
            Assert.Equal(task.Id, clone.Id);
            Assert.Equal(task.AssociatedBabyId, clone.AssociatedBabyId);
            Assert.Equal(task.TaskPriority, clone.TaskPriority);
            Assert.Equal(task.TaskName, clone.TaskName);
            Assert.Equal(task.TaskDescription, clone.TaskDescription);
            Assert.Equal(task.IsCompleted, clone.IsCompleted);
        }

        //The copy must stay an appointment: TaskListService saves clones, and a plain TaskModel would store as a task
        [Fact]
        public void Clone_Appointment_KeepsTypeAndFields()
        {
            var appointment = new AppointmentModel("Riverside Clinic", DateTime.Today.AddDays(7), new TimeSpan(9, 30, 0), new TimeSpan(10, 15, 0), 3, "Pediatrician visit", "Two-month checkup")
            {
                AssociatedBabyId = Guid.NewGuid()
            };

            AppointmentModel clone = Assert.IsType<AppointmentModel>(appointment.Clone());

            Assert.NotSame(appointment, clone);
            Assert.Equal(appointment.Id, clone.Id);
            Assert.Equal(appointment.AssociatedBabyId, clone.AssociatedBabyId);
            Assert.Equal(appointment.TaskName, clone.TaskName);
            Assert.Equal(appointment.AppointmentLocation, clone.AppointmentLocation);
            Assert.Equal(appointment.AppointmentDate, clone.AppointmentDate);
            Assert.Equal(appointment.AppointmentStartTime, clone.AppointmentStartTime);
            Assert.Equal(appointment.AppointmentEndTime, clone.AppointmentEndTime);
        }
    }
}
