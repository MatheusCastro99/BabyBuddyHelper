using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //The baby filter on the checklist and the calendar, and the appointment-only view the calendar and MainPage use
    public class TaskFilteringTests : DatabaseTest
    {
        private readonly BabyModel _olivia = TestData.Baby("Olivia");
        private readonly BabyModel _noah = TestData.Baby("Noah");

        //Two babies with a task and an appointment each, plus one unassigned task and one unassigned appointment
        private async Task<TestServices> StartWithTasksForTwoBabiesAsync()
        {
            TestServices services = await StartServicesAsync();

            await services.BabyProfiles.AddAsync(_olivia);
            await services.BabyProfiles.AddAsync(_noah);
            await services.Tasks.AddAsync(TestData.Task("Olivia task", priority: 1, babyId: _olivia.Id));
            await services.Tasks.AddAsync(TestData.Appointment("Olivia appointment", priority: 2, babyId: _olivia.Id));
            await services.Tasks.AddAsync(TestData.Task("Noah task", priority: 3, babyId: _noah.Id));
            await services.Tasks.AddAsync(TestData.Appointment("Noah appointment", priority: 4, babyId: _noah.Id));
            await services.Tasks.AddAsync(TestData.Task("Unassigned task", priority: 5));
            await services.Tasks.AddAsync(TestData.Appointment("Unassigned appointment", priority: 6));

            return services;
        }

        private static string[] Names(IEnumerable<TaskModel> tasks) => tasks.Select(x => x.TaskName).ToArray();

        [Fact]
        public async Task GetTasks_NoBaby_ReturnsEveryTaskIncludingUnassigned()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            Assert.Equal(
                ["Unassigned appointment", "Unassigned task", "Noah appointment", "Noah task", "Olivia appointment", "Olivia task"],
                Names(services.Tasks.GetTasks()));
        }

        [Fact]
        public async Task GetTasks_ForOneBaby_ReturnsOnlyThatBabysTasks()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            Assert.Equal(["Olivia appointment", "Olivia task"], Names(services.Tasks.GetTasks(_olivia.Id)));
            Assert.Equal(["Noah appointment", "Noah task"], Names(services.Tasks.GetTasks(_noah.Id)));
        }

        [Fact]
        public async Task GetTasks_ForUnknownBaby_ReturnsNothing()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            Assert.Empty(services.Tasks.GetTasks(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetTasks_ForOneBabyWithPendingFirst_FiltersThenSorts()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();
            TaskModel oliviaAppointment = Assert.Single(services.Tasks.Tasks, x => x.TaskName == "Olivia appointment");
            await services.Tasks.SetCompletionAsync(oliviaAppointment.Id, true);

            Assert.Equal(["Olivia task", "Olivia appointment"], Names(services.Tasks.GetTasks(_olivia.Id, pendingFirst: true)));
        }

        [Fact]
        public async Task GetTasks_ForRemovedBaby_ReturnsNothingAndItsTasksBecomeUnassigned()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            await services.BabyProfiles.RemoveAsync(_olivia.Id);

            Assert.Empty(services.Tasks.GetTasks(_olivia.Id));
            Assert.Equal(6, services.Tasks.GetTasks().Count());
            Assert.Equal(["Noah appointment", "Noah task"], Names(services.Tasks.GetTasks(_noah.Id)));
        }

        [Fact]
        public async Task GetAppointments_NoBaby_ReturnsOnlyAppointments()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            Assert.Equal(
                ["Unassigned appointment", "Noah appointment", "Olivia appointment"],
                Names(services.Tasks.GetAppointments()));
        }

        [Fact]
        public async Task GetAppointments_ForOneBaby_ReturnsOnlyThatBabysAppointments()
        {
            using TestServices services = await StartWithTasksForTwoBabiesAsync();

            Assert.Equal(["Olivia appointment"], Names(services.Tasks.GetAppointments(_olivia.Id)));
        }

        [Fact]
        public async Task GetAppointments_OnlyPlainTasks_ReturnsNothing()
        {
            using TestServices services = await StartServicesAsync();
            await services.Tasks.AddAsync(TestData.Task("Buy crib"));

            Assert.Empty(services.Tasks.GetAppointments());
        }
    }
}
