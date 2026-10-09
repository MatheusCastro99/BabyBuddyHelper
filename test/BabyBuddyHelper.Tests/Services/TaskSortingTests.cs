using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //The orders the checklist can show. GetTasks returns a sorted view and leaves the cache alone; the Organize methods
    //reorder the cache itself.
    public class TaskSortingTests : DatabaseTest
    {
        private static AppointmentModel AppointmentInDays(string name, int days, int priority) =>
            new("Riverside Clinic", DateTime.Today.AddDays(days), new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0), priority, name, "");

        private static string[] Names(IEnumerable<TaskModel> tasks) => tasks.Select(x => x.TaskName).ToArray();

        [Fact]
        public async Task GetTasks_Default_OrdersByHighestPriority()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(TestData.Task("Low", priority: 1));
            await services.Tasks.AddAsync(TestData.Task("High", priority: 5));
            await services.Tasks.AddAsync(TestData.Task("Medium", priority: 3));

            Assert.Equal(["High", "Medium", "Low"], Names(services.Tasks.GetTasks()));
        }

        [Fact]
        public async Task GetTasks_PendingFirst_PutsCompletedLastAndKeepsPriorityWithinEachGroup()
        {
            TaskModel doneHigh = TestData.Task("Done high", priority: 5);
            TaskModel doneLow = TestData.Task("Done low", priority: 1);

            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(doneHigh);
            await services.Tasks.AddAsync(TestData.Task("Pending low", priority: 2));
            await services.Tasks.AddAsync(doneLow);
            await services.Tasks.AddAsync(TestData.Task("Pending high", priority: 4));
            await services.Tasks.SetCompletionAsync(doneHigh.Id, true);
            await services.Tasks.SetCompletionAsync(doneLow.Id, true);

            Assert.Equal(["Pending high", "Pending low", "Done high", "Done low"], Names(services.Tasks.GetTasks(pendingFirst: true)));
        }

        [Fact]
        public async Task GetTasks_OrderByUpcomingDate_PutsAppointmentsFirstByDateThenTasksByPriority()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(TestData.Task("Task low", priority: 1));
            await services.Tasks.AddAsync(AppointmentInDays("Next month", days: 30, priority: 5));
            await services.Tasks.AddAsync(TestData.Task("Task high", priority: 4));
            await services.Tasks.AddAsync(AppointmentInDays("Tomorrow", days: 1, priority: 1));
            await services.Tasks.AddAsync(AppointmentInDays("Last week", days: -7, priority: 3));

            Assert.Equal(["Last week", "Tomorrow", "Next month", "Task high", "Task low"], Names(services.Tasks.GetTasks(orderByUpcomingDate: true)));
        }

        [Fact]
        public async Task GetTasks_OrderByUpcomingDate_SameDay_OrdersByHighestPriority()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(AppointmentInDays("Same day low", days: 2, priority: 1));
            await services.Tasks.AddAsync(AppointmentInDays("Same day high", days: 2, priority: 5));

            Assert.Equal(["Same day high", "Same day low"], Names(services.Tasks.GetTasks(orderByUpcomingDate: true)));
        }

        [Fact]
        public async Task GetTasks_AnyOrder_LeavesTheCacheOrderAlone()
        {
            TaskModel done = TestData.Task("Done high", priority: 5);

            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(done);
            await services.Tasks.AddAsync(TestData.Task("Pending low", priority: 1));
            await services.Tasks.SetCompletionAsync(done.Id, true);

            _ = services.Tasks.GetTasks(pendingFirst: true).ToList();

            Assert.Equal(["Done high", "Pending low"], Names(services.Tasks.Tasks));
        }

        [Fact]
        public async Task AddAsync_AnyOrder_KeepsTheCacheOrderedByHighestPriority()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(TestData.Task("Low", priority: 1));
            await services.Tasks.AddAsync(TestData.Task("High", priority: 5));
            await services.Tasks.AddAsync(TestData.Task("Medium", priority: 3));

            Assert.Equal(["High", "Medium", "Low"], Names(services.Tasks.Tasks));
        }

        [Fact]
        public async Task AddAsync_SamePriority_GoesAfterTheOnesAlreadyThere()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(TestData.Task("First", priority: 3));
            await services.Tasks.AddAsync(TestData.Task("Second", priority: 3));
            await services.Tasks.AddAsync(TestData.Task("Third", priority: 3));

            Assert.Equal(["First", "Second", "Third"], Names(services.Tasks.Tasks));
        }

        [Fact]
        public async Task InitializeAsync_StoredTasks_LoadsThemOrderedByHighestPriority()
        {
            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(TestData.Task("Low", priority: 1));
                await services.Tasks.AddAsync(TestData.Task("High", priority: 5));
                await services.Tasks.AddAsync(TestData.Task("Medium", priority: 3));
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal(["High", "Medium", "Low"], Names(restarted.Tasks.Tasks));
        }

        [Fact]
        public async Task OrganizeByPending_MixedTasks_MovesCompletedToTheEndOfTheCache()
        {
            TaskModel doneHigh = TestData.Task("Done high", priority: 5);
            TaskModel pendingLow = TestData.Task("Pending low", priority: 1);
            TaskModel pendingMedium = TestData.Task("Pending medium", priority: 3);

            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(doneHigh);
            await services.Tasks.AddAsync(pendingLow);
            await services.Tasks.AddAsync(pendingMedium);
            await services.Tasks.SetCompletionAsync(doneHigh.Id, true);

            services.Tasks.OrganizeByPending();

            //The same instances, moved: nothing is replaced or dropped
            Assert.Equal([pendingMedium, pendingLow, doneHigh], services.Tasks.Tasks);
        }

        [Fact]
        public async Task OrganizeByPriority_AfterOrganizeByPending_RestoresThePriorityOrder()
        {
            TaskModel doneHigh = TestData.Task("Done high", priority: 5);

            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(doneHigh);
            await services.Tasks.AddAsync(TestData.Task("Pending low", priority: 1));
            await services.Tasks.AddAsync(TestData.Task("Pending medium", priority: 3));
            await services.Tasks.SetCompletionAsync(doneHigh.Id, true);
            services.Tasks.OrganizeByPending();

            services.Tasks.OrganizeByPriority();

            Assert.Equal(["Done high", "Pending medium", "Pending low"], Names(services.Tasks.Tasks));
        }

        [Fact]
        public async Task Organize_EmptyCache_DoesNothing()
        {
            using TestServices services = await StartServicesAsync();

            services.Tasks.OrganizeByPriority();
            services.Tasks.OrganizeByPending();

            Assert.Empty(services.Tasks.Tasks);
        }
    }
}
