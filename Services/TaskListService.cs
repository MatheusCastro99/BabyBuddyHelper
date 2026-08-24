using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BabyBuddyHelper.Services
{
    internal class TaskListService : ITaskListService //Implemente Interface ITaskListService to provide functionality for managing a list of tasks.
    {                                                 //This class will be used to add, remove, update, and organize tasks in the application.
        public ObservableCollection<TaskModel> Tasks { get; } = new();

        public TaskListService()
        {
            if(Tasks == null)
            {
                GenerateMockData();
            }
        }

        public void Add(TaskModel task)
        {
            Tasks.Add(task);
        }

        public void Remove(TaskModel task)
        {
            Tasks.Remove(task);
        }

        public void Update(TaskModel updatedTask)
        {
            var existingTask = Tasks.FirstOrDefault(x => x.Id == updatedTask.Id);

            if (existingTask is null)
            return;

            var index = Tasks.IndexOf(existingTask);
            Tasks[index] = updatedTask;
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
            Tasks.Clear();

            foreach (var task in sortedTasks)
            {
                Tasks.Add(task);
            }
        }

        private void GenerateMockData() //Method to generate mock data for testing purposes.
        {                              //This will be called in the constructor of the ChecklistPage to populate the list with some initial tasks.
            Tasks.Add(new TaskModel(5, "Organize Room", "Make Space for the baby!"));
            Tasks.Add(new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!"));
            Tasks.Add(new AppointmentModel("NJ", new(2026, 09, 15), new(23, 0, 0), new(23, 30, 0), 7, "BabyShower", "Get gifts"));
            Tasks.Add(new AppointmentModel("Hospotal", new(2026, 09, 25), new(09, 15, 0), new(10, 0, 0), 8, "Imaging", "See the baby!"));
        }
    }
}
