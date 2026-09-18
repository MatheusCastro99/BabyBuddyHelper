namespace BabyBuddyHelper.Models
{
    public class TaskModel
    {
        //Properties Definition
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid? AssociatedBabyId { get; set; } //Only the Id is stored. The display name is resolved from IBabyProfileService when shown.
        public int TaskPriority { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

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
