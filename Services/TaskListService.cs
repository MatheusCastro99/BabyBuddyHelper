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
        public ObservableCollection<TaskModel> Tasks { get; } = new();
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

            if (Tasks.Count == 0)
            {
                GenerateMockData();
            }
        }

        //Writes update the in-memory collection first so the UI responds instantly, then persist through ITrackerDbService.
        public async Task AddAsync(TaskModel task)
        {
            Tasks.Add(task);
            OrganizeByPriority();
            await _trackerDbService.AddTaskAsync(task);
        }

        public async Task RemoveAsync(Guid taskId)
        {
            var existingTask = Tasks.FirstOrDefault(x => x.Id == taskId);

            if (existingTask is null)
                return;

            Tasks.Remove(existingTask);
            await _trackerDbService.RemoveTaskAsync(taskId);
        }

        //The replacement may be a different concrete type (task <-> appointment conversion); the entry keeps its Id and slot
        public async Task UpdateAsync(TaskModel task)
        {
            var existingTask = Tasks.FirstOrDefault(x => x.Id == task.Id);

            if (existingTask is null)
                return;

            var index = Tasks.IndexOf(existingTask);
            Tasks[index] = task;
            await _trackerDbService.UpdateTaskAsync(task);
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
        //Removals only update the in-memory copy. The database clears the deleted baby from stored tasks itself (see #25).
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
            if (task is AppointmentModel appointment)
            {
                return new AppointmentModel(
                    appointment.AppointmentLocation,
                    appointment.AppointmentDate,
                    appointment.AppointmentStartTime,
                    appointment.AppointmentEndTime,
                    appointment.TaskPriority,
                    appointment.TaskName,
                    appointment.TaskDescription)
                {
                    Id = appointment.Id,
                    IsCompleted = appointment.IsCompleted,
                    AssociatedBabyId = associatedBabyId
                };
            }

            return new TaskModel(task.TaskPriority, task.TaskName, task.TaskDescription)
            {
                Id = task.Id,
                IsCompleted = task.IsCompleted,
                AssociatedBabyId = associatedBabyId
            };
        }

        public void Dispose()
        {
            _babyProfileService.BabyProfiles.CollectionChanged -= OnBabyProfilesChanged;
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
