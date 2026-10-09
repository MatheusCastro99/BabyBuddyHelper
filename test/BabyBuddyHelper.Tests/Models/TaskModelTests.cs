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
    }
}
