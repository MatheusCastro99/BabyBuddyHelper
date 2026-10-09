using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //Records that look identical in every field except their Id. Anything that found a record by its name or by comparing
    //values would hit the wrong one here; only the Id tells them apart (ADR-007).
    public class CollidingDataTests : DatabaseTest
    {
        [Fact]
        public async Task UpdateAsync_OneOfTwoIdenticalTasks_ChangesOnlyThatOne()
        {
            TaskModel first = TestData.Task("Buy diapers");
            TaskModel second = TestData.Task("Buy diapers");
            var editedSecond = new TaskModel(second.TaskPriority, "Buy wipes", second.TaskDescription) { Id = second.Id };

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(first);
                await services.Tasks.AddAsync(second);

                await services.Tasks.UpdateAsync(editedSecond);

                Assert.Equal("Buy diapers", Assert.Single(services.Tasks.Tasks, x => x.Id == first.Id).TaskName);
                Assert.Equal("Buy wipes", Assert.Single(services.Tasks.Tasks, x => x.Id == second.Id).TaskName);
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal("Buy diapers", Assert.Single(restarted.Tasks.Tasks, x => x.Id == first.Id).TaskName);
            Assert.Equal("Buy wipes", Assert.Single(restarted.Tasks.Tasks, x => x.Id == second.Id).TaskName);
        }

        [Fact]
        public async Task SetCompletionAsync_OneOfTwoIdenticalTasks_CompletesOnlyThatOne()
        {
            TaskModel first = TestData.Task("Buy diapers");
            TaskModel second = TestData.Task("Buy diapers");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(first);
                await services.Tasks.AddAsync(second);

                await services.Tasks.SetCompletionAsync(second.Id, true);
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.False(Assert.Single(restarted.Tasks.Tasks, x => x.Id == first.Id).IsCompleted);
            Assert.True(Assert.Single(restarted.Tasks.Tasks, x => x.Id == second.Id).IsCompleted);
        }

        [Fact]
        public async Task RemoveAsync_OneOfTwoIdenticalTasks_RemovesOnlyThatOne()
        {
            TaskModel first = TestData.Task("Buy diapers");
            TaskModel second = TestData.Task("Buy diapers");

            using (TestServices services = await StartServicesAsync())
            {
                await services.Tasks.AddAsync(first);
                await services.Tasks.AddAsync(second);

                await services.Tasks.RemoveAsync(first.Id);

                Assert.Same(second, Assert.Single(services.Tasks.Tasks));
            }

            using TestServices restarted = await StartServicesAsync();
            Assert.Equal(second.Id, Assert.Single(restarted.Tasks.Tasks).Id);
        }

        //Twins, or two profiles entered with the same name
        [Fact]
        public async Task UpdateAsync_OneOfTwoBabiesWithTheSameName_ChangesOnlyThatOne()
        {
            BabyModel first = TestData.Baby("Sam");
            BabyModel second = TestData.Baby("Sam");
            var editedFirst = new BabyModel { Id = first.Id, Name = "Samuel", DateOfBirth = first.DateOfBirth };

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(first);
                await services.BabyProfiles.AddAsync(second);

                await services.BabyProfiles.UpdateAsync(editedFirst);
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal("Samuel", Assert.Single(restarted.BabyProfiles.BabyProfiles, x => x.Id == first.Id).Name);
            Assert.Equal("Sam", Assert.Single(restarted.BabyProfiles.BabyProfiles, x => x.Id == second.Id).Name);
        }

        [Fact]
        public async Task RemoveAsync_OneOfTwoBabiesWithTheSameName_KeepsTheOtherAndItsData()
        {
            BabyModel removed = TestData.Baby("Sam");
            BabyModel kept = TestData.Baby("Sam");
            TaskModel keptTask = TestData.Task("Buy diapers", babyId: kept.Id);
            VaccinationRecordModel keptRecord = TestData.Record(kept.Id);

            using (TestServices services = await StartServicesAsync())
            {
                await services.BabyProfiles.AddAsync(removed);
                await services.BabyProfiles.AddAsync(kept);
                await services.Tasks.AddAsync(TestData.Task("Buy diapers", babyId: removed.Id));
                await services.Tasks.AddAsync(keptTask);
                await services.Vaccines.AddAsync(TestData.Record(removed.Id));
                await services.Vaccines.AddAsync(keptRecord);

                await services.BabyProfiles.RemoveAsync(removed.Id);

                Assert.Same(kept, Assert.Single(services.BabyProfiles.BabyProfiles));
                Assert.Equal(kept.Id, Assert.Single(services.Tasks.Tasks, x => x.Id == keptTask.Id).AssociatedBabyId);
                Assert.Same(keptRecord, Assert.Single(services.Vaccines.VaccinationRecords));
            }

            using TestServices restarted = await StartServicesAsync();

            Assert.Equal(kept.Id, Assert.Single(restarted.BabyProfiles.BabyProfiles).Id);
            Assert.Equal(kept.Id, Assert.Single(restarted.Tasks.Tasks, x => x.Id == keptTask.Id).AssociatedBabyId);
            Assert.Equal(keptRecord.Id, Assert.Single(restarted.Vaccines.VaccinationRecords).Id);
        }
    }
}
