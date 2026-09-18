using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;

namespace BabyBuddyHelper.Services
{
    //Stand-in for the database until the EF Core SQLite implementation lands (#25). Data lives only for the app session.
    public class InMemoryTrackerDbService : ITrackerDbService
    {
        private readonly List<TaskModel> _tasks = new();
        private readonly List<BabyModel> _babyProfiles = new();

        public Task<IReadOnlyList<TaskModel>> GetTasksAsync()
        {
            return Task.FromResult<IReadOnlyList<TaskModel>>(_tasks.ToList());
        }

        public Task AddTaskAsync(TaskModel task)
        {
            _tasks.Add(task);
            return Task.CompletedTask;
        }

        public Task UpdateTaskAsync(TaskModel task)
        {
            int index = _tasks.FindIndex(x => x.Id == task.Id); //Replacing the whole entry also covers task <-> appointment conversion

            if (index >= 0)
            {
                _tasks[index] = task;
            }

            return Task.CompletedTask;
        }

        public Task RemoveTaskAsync(Guid taskId)
        {
            _tasks.RemoveAll(x => x.Id == taskId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<BabyModel>> GetBabyProfilesAsync()
        {
            return Task.FromResult<IReadOnlyList<BabyModel>>(_babyProfiles.ToList());
        }

        public Task AddBabyProfileAsync(BabyModel babyProfile)
        {
            _babyProfiles.Add(babyProfile);
            return Task.CompletedTask;
        }

        public Task UpdateBabyProfileAsync(BabyModel babyProfile)
        {
            int index = _babyProfiles.FindIndex(x => x.Id == babyProfile.Id);

            if (index >= 0)
            {
                _babyProfiles[index] = babyProfile;
            }

            return Task.CompletedTask;
        }

        public Task RemoveBabyProfileAsync(Guid babyId)
        {
            _babyProfiles.RemoveAll(x => x.Id == babyId);
            return Task.CompletedTask;
        }
    }
}
