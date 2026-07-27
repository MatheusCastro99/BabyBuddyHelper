using System;
using System.Collections.Generic;
using System.Text;

namespace BabyPrepRegistry.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public int TaskPriority { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }

        public TaskModel(int priority, string taskName, string taskDescription) 
        {
            TaskPriority = priority;
            TaskName = taskName;
            TaskDescription = taskDescription;
            IsCompleted = false;
        }
    }
}
