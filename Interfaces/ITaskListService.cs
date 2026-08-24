using BabyBuddyHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BabyBuddyHelper.Interfaces
{
    internal interface ITaskListService //Setting interface for TaskListService to implement. This allows for dependency injection, easier testing,
                                        // and better encapsulation.
    {
        ObservableCollection<TaskModel> Tasks { get; }
        void Add(TaskModel task);
        void Remove(TaskModel task);
        void Update(TaskModel task);
        void OrganizeByPriority();
        void OrganizeByPending();
    }
}
