using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;

namespace BabyBuddyHelper.Services
{
    //Seeds mock tasks into the database for development. Debug builds only, and only when the store has no tasks,
    //so relaunching never duplicates them. Writes go through ITrackerDbService, exercising the real persistence path.
    //The check-then-insert is not atomic: callers must not run it concurrently. App's single startup run guarantees that.
    public static class TrackerDataSeeder
    {
        public static async Task SeedIfEmptyAsync(ITrackerDbService trackerDbService)
        {
#if DEBUG
            IReadOnlyList<TaskModel> existingTasks = await trackerDbService.GetTasksAsync();

            if (existingTasks.Count > 0)
            {
                return;
            }

            foreach (TaskModel mockTask in CreateMockTasks())
            {
                await trackerDbService.AddTaskAsync(mockTask);
            }
#else
            await Task.CompletedTask;
#endif
        }

#if DEBUG
        private static IEnumerable<TaskModel> CreateMockTasks()
        {
            yield return new TaskModel(5, "Organize Room", "Make Space for the baby!");
            yield return new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!");
            yield return new AppointmentModel("NJ", new(2026, 09, 15, 0, 0, 0, DateTimeKind.Local), new(23, 0, 0), new(23, 30, 0), 7, "BabyShower", "Get gifts");
            yield return new AppointmentModel("Hospital", new(2026, 09, 25, 0, 0, 0, DateTimeKind.Local), new(09, 15, 0), new(10, 0, 0), 8, "Imaging", "See the baby!");
            yield return new AppointmentModel("Home", new(2026, 08, 25, 0, 0, 0, DateTimeKind.Local), new(09, 15, 0), new(10, 0, 0), 8, "Chilling", "Testing some stuff");
            yield return new AppointmentModel("In my pc", new(2026, 08, 26, 0, 0, 0, DateTimeKind.Local), new(10, 0, 0), new(11, 0, 0), 8, "Testing", "Will it bind now?");
            yield return new AppointmentModel("Bed", new(2026, 08, 24, 0, 0, 0, DateTimeKind.Local), new(20, 0, 0), new(21, 30, 0), 8, "Sleep", "Or try to");
        }
#endif
    }
}
