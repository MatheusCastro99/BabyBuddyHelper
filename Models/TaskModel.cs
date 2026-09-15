namespace BabyBuddyHelper.Models
{
    public class TaskModel
    {
        //Properties Definition
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid? AssociatedBabyId { get; set; }
        public string AssociatedBabyName { get; set; } = "General";
        public int TaskPriority { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
        public string AssociatedBabyDisplayName => string.IsNullOrWhiteSpace(AssociatedBabyName) ? "General" : AssociatedBabyName;
        public string TaskDisplayName => $"{TaskName} - {AssociatedBabyDisplayName}";

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
