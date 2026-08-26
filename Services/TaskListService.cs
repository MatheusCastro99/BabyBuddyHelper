using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Services
{
    public class TaskListService : ITaskListService //Implemente Interface ITaskListService to provide functionality for managing a list of tasks.
    {                                                 //This class will be used to add, remove, update, and organize tasks in the application.
        public ObservableCollection<TaskModel> Tasks { get; } = new();
        public IEnumerable<AppointmentModel> GetAppointments() //return a list of AppointmentModel objects from the Tasks collection, 
        {                                                       //filtering out any tasks that are not of type AppointmentModel.
            return Tasks.OfType<AppointmentModel>();             //Will be used on scheduler to display appointments in a calendar view.
        }

        public TaskListService()
        {
            if (Tasks.Count == 0)
            {
                GenerateMockData();
            }
        }

        public void Add(TaskModel task)
        {
            Tasks.Add(task);
            OrganizeByPriority();
        }

        public void Remove(TaskModel task)
        {
            Tasks.Remove(task);
        }

        public void Update(TaskModel task)
        {
            var existingTask = Tasks.FirstOrDefault(x => x.Id == task.Id);

            if (existingTask is null)
                return;

            var index = Tasks.IndexOf(existingTask);
            Tasks[index] = task;
        }

        public void OrganizeByPriority()
        {
            var sortedList = Tasks
            .OrderByDescending(x => x.TaskPriority)
            .ToList();

            RebuildCollection(sortedList);
        }

        public void OrganizeByPending()
        {
            var sortedList = Tasks
            .OrderBy(x => x.IsCompleted)
            .ThenByDescending(x => x.TaskPriority)
            .ToList();

            RebuildCollection(sortedList);
        }

        private void RebuildCollection(List<TaskModel> sortedTasks)
        {
            for (int targetIndex = 0; targetIndex < sortedTasks.Count; targetIndex++)
            {
                var item = sortedTasks[targetIndex];
                var currentIndex = Tasks.IndexOf(item);

                if (currentIndex != targetIndex)
                {
                    Tasks.Move(currentIndex, targetIndex);
                }
            }
        }

        private void GenerateMockData() //Method to generate mock data for testing purposes.
        {                              //This will be called in the constructor of the ChecklistPage to populate the list with some initial tasks.
            Tasks.Add(new TaskModel(5, "Organize Room", "Make Space for the baby!"));
            Tasks.Add(new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!"));
            Tasks.Add(new AppointmentModel("NJ", new(2026, 09, 15, 0, 0, 0, DateTimeKind.Local), new(23, 0, 0), new(23, 30, 0), 7, "BabyShower", "Get gifts"));
            Tasks.Add(new AppointmentModel("Hospotal", new(2026, 09, 25, 0, 0, 0, DateTimeKind.Local), new(09, 15, 0), new(10, 0, 0), 8, "Imaging", "See the baby!"));
            Tasks.Add(new AppointmentModel("Home", new(2026, 08, 25, 0, 0, 0, DateTimeKind.Local), new(09, 15, 0), new(10, 0, 0), 8, "Chilling", "Testing some stuff"));
            Tasks.Add(new AppointmentModel("In my pc", new(2026, 08, 26, 0, 0, 0, DateTimeKind.Local), new(10, 0, 0), new(11, 0, 0), 8, "Testing", "Will it bind now?"));
            Tasks.Add(new AppointmentModel("Bed", new(2026, 08, 24, 0, 0, 0, DateTimeKind.Local), new(20, 0, 0), new(21, 30, 0), 8, "Sleep", "Or try to"));
        }
    }
}
