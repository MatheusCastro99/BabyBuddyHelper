using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Core.Services;
using BabyBuddyHelper.Tests.Infrastructure;

namespace BabyBuddyHelper.Tests.Services
{
    //The baby option lists shown by the checklist and calendar filters ("All babies") and the task editor ("Unassigned")
    public class BabyFilterServiceTests : DatabaseTest
    {
        private const string FirstOption = "All babies";

        [Fact]
        public async Task BuildOptions_NoBabies_HasOnlyTheFirstOption()
        {
            using TestServices services = await StartServicesAsync();
            var filter = new BabyFilterService(services.BabyProfiles);

            KeyValuePair<string, Guid?> option = Assert.Single(filter.BuildOptions(FirstOption));

            Assert.Equal(FirstOption, option.Key);
            Assert.Null(option.Value);
        }

        [Fact]
        public async Task BuildOptions_SeveralBabies_ListsTheFirstOptionThenEachBabyInProfileOrder()
        {
            BabyModel olivia = TestData.Baby("Olivia");
            BabyModel noah = TestData.Baby("Noah");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(olivia);
            await services.BabyProfiles.AddAsync(noah);
            var filter = new BabyFilterService(services.BabyProfiles);

            Dictionary<string, Guid?> options = filter.BuildOptions(FirstOption);

            Assert.Equal([FirstOption, "Olivia", "Noah"], options.Keys);
            Assert.Null(options[FirstOption]);
            Assert.Equal(olivia.Id, options["Olivia"]);
            Assert.Equal(noah.Id, options["Noah"]);
        }

        [Fact]
        public async Task BuildOptions_BabiesWithTheSameName_NumbersTheLaterOnes()
        {
            BabyModel first = TestData.Baby("Sam");
            BabyModel second = TestData.Baby("Sam");
            BabyModel third = TestData.Baby("Sam");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(first);
            await services.BabyProfiles.AddAsync(second);
            await services.BabyProfiles.AddAsync(third);
            var filter = new BabyFilterService(services.BabyProfiles);

            Dictionary<string, Guid?> options = filter.BuildOptions(FirstOption);

            Assert.Equal([FirstOption, "Sam", "Sam (2)", "Sam (3)"], options.Keys);
            Assert.Equal(first.Id, options["Sam"]);
            Assert.Equal(second.Id, options["Sam (2)"]);
            Assert.Equal(third.Id, options["Sam (3)"]);
        }

        //A baby whose real name is already a numbered label must not be overwritten by the numbering
        [Fact]
        public async Task BuildOptions_NameThatLooksLikeANumberedLabel_KeepsEveryBaby()
        {
            BabyModel sam = TestData.Baby("Sam");
            BabyModel namedLikeALabel = TestData.Baby("Sam (2)");
            BabyModel secondSam = TestData.Baby("Sam");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(sam);
            await services.BabyProfiles.AddAsync(namedLikeALabel);
            await services.BabyProfiles.AddAsync(secondSam);
            var filter = new BabyFilterService(services.BabyProfiles);

            Dictionary<string, Guid?> options = filter.BuildOptions(FirstOption);

            Assert.Equal(4, options.Count);
            Assert.Equal(sam.Id, options["Sam"]);
            Assert.Equal(namedLikeALabel.Id, options["Sam (2)"]);
            Assert.Equal(secondSam.Id, options["Sam (3)"]);
        }

        //The first option keeps its label and its "no baby" meaning even when a baby has the same name
        [Fact]
        public async Task BuildOptions_BabyNamedLikeTheFirstOption_NumbersTheBaby()
        {
            BabyModel baby = TestData.Baby(FirstOption);

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(baby);
            var filter = new BabyFilterService(services.BabyProfiles);

            Dictionary<string, Guid?> options = filter.BuildOptions(FirstOption);

            Assert.Null(options[FirstOption]);
            Assert.Equal(baby.Id, options[$"{FirstOption} (2)"]);
        }

        [Fact]
        public async Task ResolveSelection_NothingSelected_ReturnsTheFirstOption()
        {
            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(TestData.Baby("Olivia"));
            var filter = new BabyFilterService(services.BabyProfiles);

            Assert.Equal(((Guid?)null, FirstOption), filter.ResolveSelection(null, FirstOption));
        }

        [Fact]
        public async Task ResolveSelection_ExistingBaby_ReturnsItsIdAndName()
        {
            BabyModel olivia = TestData.Baby("Olivia");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(TestData.Baby("Noah"));
            await services.BabyProfiles.AddAsync(olivia);
            var filter = new BabyFilterService(services.BabyProfiles);

            Assert.Equal(((Guid?)olivia.Id, "Olivia"), filter.ResolveSelection(olivia.Id, FirstOption));
        }

        [Fact]
        public async Task ResolveSelection_SecondBabyWithTheSameName_ReturnsItsNumberedLabel()
        {
            BabyModel second = TestData.Baby("Sam");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(TestData.Baby("Sam"));
            await services.BabyProfiles.AddAsync(second);
            var filter = new BabyFilterService(services.BabyProfiles);

            Assert.Equal(((Guid?)second.Id, "Sam (2)"), filter.ResolveSelection(second.Id, FirstOption));
        }

        //A page can still hold the Id of a baby that was deleted meanwhile; it falls back to the first option
        [Fact]
        public async Task ResolveSelection_RemovedBaby_FallsBackToTheFirstOption()
        {
            BabyModel olivia = TestData.Baby("Olivia");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(olivia);
            var filter = new BabyFilterService(services.BabyProfiles);
            await services.BabyProfiles.RemoveAsync(olivia.Id);

            Assert.Equal(((Guid?)null, FirstOption), filter.ResolveSelection(olivia.Id, FirstOption));
        }

        [Fact]
        public async Task BuildOptions_AfterARename_ShowsTheNewName()
        {
            BabyModel olivia = TestData.Baby("Olivia");

            using TestServices services = await StartServicesAsync();
            await services.BabyProfiles.AddAsync(olivia);
            var filter = new BabyFilterService(services.BabyProfiles);
            await services.BabyProfiles.UpdateAsync(new BabyModel { Id = olivia.Id, Name = "Olivia Rose", DateOfBirth = olivia.DateOfBirth });

            Assert.Equal([FirstOption, "Olivia Rose"], filter.BuildOptions(FirstOption).Keys);
        }
    }
}
