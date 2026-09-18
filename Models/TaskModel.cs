namespace BabyBuddyHelper.Models
{
    public class TaskModel
    {
        //Properties Definition
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid? AssociatedBabyId { get; set; }
        public string AssociatedBabyName { get; set; } = "General";
        public int TaskPriority { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string AssociatedBabyDisplayName => string.IsNullOrWhiteSpace(AssociatedBabyName) ? "General" : AssociatedBabyName;
        public string TaskDisplayName => $"{TaskName} - {AssociatedBabyDisplayName}";

        //Parameterless constructor for database materialization (EF Core). App code uses the constructor below.
        protected TaskModel()
        {
        }

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
