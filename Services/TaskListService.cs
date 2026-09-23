using BabyBuddyHelper.Collections;
using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BabyBuddyHelper.Services
{
    public class TaskListService : ITaskListService, IDisposable //Implemente Interface ITaskListService to provide functionality for managing a list of tasks.
    {                                                 //This class will be used to add, remove, update, and organize tasks in the application.
        private readonly IBabyProfileService _babyProfileService;
        private readonly ITrackerDbService _trackerDbService;
        private Task? _initializationTask;
        private readonly RangeObservableCollection<TaskModel> _tasks = new();
        public ObservableCollection<TaskModel> Tasks => _tasks;
        public IEnumerable<TaskModel> GetTasks(Guid? associatedBabyId = null, bool pendingFirst = false, bool orderByUpcomingDate = false)
        {
            IEnumerable<TaskModel> filteredTasks = Tasks
                .Where(task => associatedBabyId is null || task.AssociatedBabyId == associatedBabyId);

            if (orderByUpcomingDate)
            {
                return filteredTasks
                    .OrderBy(task => task is AppointmentModel appointment && appointment.AppointmentDate.HasValue ? 0 : 1)
                    .ThenBy(task => (task as AppointmentModel)?.AppointmentDate ?? DateTime.MaxValue)
                    .ThenByDescending(task => task.TaskPriority);
            }

            if (pendingFirst)
            {
                return filteredTasks
                    .OrderBy(task => task.IsCompleted)
                    .ThenByDescending(task => task.TaskPriority);
            }

            return filteredTasks
                .OrderByDescending(task => task.TaskPriority);
        }

        public IEnumerable<AppointmentModel> GetAppointments(Guid? associatedBabyId = null) //return a list of AppointmentModel objects from the Tasks collection, 
        {                                                                                    //filtering out any tasks that are not of type AppointmentModel.
            return GetTasks(associatedBabyId).OfType<AppointmentModel>();                    //Will be used on scheduler to display appointments in a calendar view.
        }

        public TaskListService(IBabyProfileService babyProfileService, ITrackerDbService trackerDbService)
        {
            _babyProfileService = babyProfileService;
            _trackerDbService = trackerDbService;
            babyProfileService.BabyProfiles.CollectionChanged += OnBabyProfilesChanged;
        }

        //Loads the cache from the database once at startup. Concurrent callers share the same load, so a re-created window
        //can't duplicate entries; a failed load is retried on the next call instead of leaving the cache empty for the session.
        public Task InitializeAsync()
        {
            if (_initializationTask is null || _initializationTask.IsFaulted || _initializationTask.IsCanceled)
            {
                _initializationTask = LoadTasksAsync();
            }

            return _initializationTask;
        }

        //Sorted before publishing, so the pages get one Reset instead of one event per task plus one per reorder
        private async Task LoadTasksAsync()
        {
            IReadOnlyList<TaskModel> storedTasks = await _trackerDbService.GetTasksAsync();
            _tasks.AddRange(storedTasks.OrderByDescending(x => x.TaskPriority));
        }

        //Writes persist through ITrackerDbService first; the cache only changes once the database has committed. A failed save
        //throws DbCommunicationException before the cache is touched, so it never shows data that wasn't stored.
        //After the await, entries are found again by Id (ADR-007): the instance found before the save may have been replaced meanwhile.
        public async Task AddAsync(TaskModel task)
        {
            await _trackerDbService.AddTaskAsync(task);
            Tasks.Add(task);
            OrganizeByPriority();
        }

        public async Task RemoveAsync(Guid taskId)
        {
            if (!Tasks.Any(x => x.Id == taskId))
                return;

            await _trackerDbService.RemoveTaskAsync(taskId);

            var removedTask = Tasks.FirstOrDefault(x => x.Id == taskId);

            if (removedTask is not null)
            {
                Tasks.Remove(removedTask);
            }
        }

        //The replacement may be a different concrete type (task <-> appointment conversion); the entry keeps its Id and slot
        public async Task UpdateAsync(TaskModel task)
        {
            if (!Tasks.Any(x => x.Id == task.Id))
                return;

            await _trackerDbService.UpdateTaskAsync(task);

            var existingTask = Tasks.FirstOrDefault(x => x.Id == task.Id);

            if (existingTask is not null)
            {
                Tasks[Tasks.IndexOf(existingTask)] = task;
            }
        }

        //Saves a copy carrying the new state, so the cached entry only changes after the database commits. The cached entry is
        //then mutated in place rather than replaced: a replacement would raise CollectionChanged and rebuild the checklist on
        //every tick (scroll jump, rows reordering under the user's finger). Pages that need the new count refresh on appearing.
        public async Task SetCompletionAsync(Guid taskId, bool isCompleted)
        {
            var existingTask = Tasks.FirstOrDefault(x => x.Id == taskId);

            if (existingTask is null || existingTask.IsCompleted == isCompleted)
                return;

            TaskModel updatedTask = existingTask.Clone();
            updatedTask.IsCompleted = isCompleted;
            await _trackerDbService.UpdateTaskAsync(updatedTask);

            var cachedTask = Tasks.FirstOrDefault(x => x.Id == taskId);

            if (cachedTask is not null)
            {
                cachedTask.IsCompleted = isCompleted;
            }
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

        //Renames need no handling here: tasks only store the baby Id, and pages resolve the name when displaying it.
        //Removals only update the in-memory copy. By the time a profile leaves the cache, the database has already cleared it from
        //the stored tasks (ON DELETE SET NULL, see TrackerContext), so this mirrors a change that is already saved.
        private void OnBabyProfilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (BabyModel removedProfile in e.OldItems.OfType<BabyModel>())
                {
                    ClearAssociatedBaby(removedProfile.Id);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearMissingBabyAssociations();
            }
        }

        private void ClearAssociatedBaby(Guid babyId)
        {
            for (int taskIndex = 0; taskIndex < Tasks.Count; taskIndex++)
            {
                if (Tasks[taskIndex].AssociatedBabyId != babyId)
                {
                    continue;
                }

                Tasks[taskIndex] = CloneWithAssociation(Tasks[taskIndex], null);
            }
        }

        private void ClearMissingBabyAssociations()
        {
            HashSet<Guid> activeBabyIds = _babyProfileService.BabyProfiles
                .Select(profile => profile.Id)
                .ToHashSet();

            for (int taskIndex = 0; taskIndex < Tasks.Count; taskIndex++)
            {
                Guid? associatedBabyId = Tasks[taskIndex].AssociatedBabyId;

                if (!associatedBabyId.HasValue || activeBabyIds.Contains(associatedBabyId.Value))
                {
                    continue;
                }

                Tasks[taskIndex] = CloneWithAssociation(Tasks[taskIndex], null);
            }
        }

        //Replaces the entry instead of mutating it, so CollectionChanged fires and every page refreshes (see ADR-007)
        private static TaskModel CloneWithAssociation(TaskModel task, Guid? associatedBabyId)
        {
            TaskModel clone = task.Clone();
            clone.AssociatedBabyId = associatedBabyId;
            return clone;
        }

        public void Dispose()
        {
            _babyProfileService.BabyProfiles.CollectionChanged -= OnBabyProfilesChanged;
        }
    }
}
