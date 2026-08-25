namespace BabyBuddyHelper.Models
{
    public class TaskModel
    {
        //Properties Definition
        public Guid Id { get; init; } = Guid.NewGuid();
        public int TaskPriority { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }

        //Constructor
        public TaskModel(int priority, string taskName, string taskDescription)
        {
            TaskPriority = priority;
            TaskName = taskName;
            TaskDescription = taskDescription;
            IsCompleted = false;
        }
    }
}
