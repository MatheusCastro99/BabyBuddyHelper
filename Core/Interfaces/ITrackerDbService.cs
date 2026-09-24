using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Core.Interfaces
{
    //Persistence boundary for tasks, appointments, and baby profiles. Only the cache services (TaskListService,
    //BabyProfileService) talk to it; pages never do. Records are located by Id (ADR-007).
    //Every method throws DbCommunicationException when the database can't be reached or rejects the command.
    public interface ITrackerDbService
    {
        Task<IReadOnlyList<TaskModel>> GetTasksAsync();
        Task AddTaskAsync(TaskModel task);

        //May change the record's concrete type (task <-> appointment conversion). Implementations handle that as one operation.
        Task UpdateTaskAsync(TaskModel task);
        Task RemoveTaskAsync(Guid taskId);

        Task<IReadOnlyList<BabyModel>> GetBabyProfilesAsync();
        Task AddBabyProfileAsync(BabyModel babyProfile);
        Task UpdateBabyProfileAsync(BabyModel babyProfile);
        Task RemoveBabyProfileAsync(Guid babyId);
    }
}
