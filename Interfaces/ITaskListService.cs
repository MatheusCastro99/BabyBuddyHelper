using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Interfaces
{
    public interface ITaskListService //Setting interface for TaskListService to implement. This allows for dependency injection, easier testing,
                                      // and better encapsulation.
    {
        ObservableCollection<TaskModel> Tasks { get; }
        Task InitializeAsync();
        Task AddAsync(TaskModel task);
        Task RemoveAsync(Guid taskId);
        Task UpdateAsync(TaskModel task);
        Task SetCompletionAsync(Guid taskId, bool isCompleted);
        void OrganizeByPriority();
        void OrganizeByPending();
        IEnumerable<TaskModel> GetTasks(Guid? associatedBabyId = null, bool pendingFirst = false, bool orderByUpcomingDate = false);
        IEnumerable<AppointmentModel> GetAppointments(Guid? associatedBabyId = null);
    }
}
