using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BabyBuddyHelper.Services
{
    internal class TaskListService : ITaskListService //Implemente Interface ITaskListService to provide functionality for managing a list of tasks.
    {                                                 //This class will be used to add, remove, update, and organize tasks in the application.
        public ObservableCollection<TaskModel> Tasks { get; } = new();

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
    }
}
