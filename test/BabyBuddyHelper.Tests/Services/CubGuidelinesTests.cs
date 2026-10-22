using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Core.Services;

namespace BabyBuddyHelper.Tests.Services
{
    //Cub's fixed rules, the baby block and the history trim (#96). The rules are checked by a marker phrase each;
    //whether Claude follows them is the manual checklist's job (CUB_GUIDELINES_CHECKLIST.md).
    public class CubGuidelinesTests
    {
        private static readonly VaccineCatalog Catalog = new();

        private static ChatTurn Parent(string text) => new(ChatSpeaker.Parent, text);
        private static ChatTurn Cub(string text) => new(ChatSpeaker.Cub, text);

        [Theory]
        [InlineData("You are Cub")] //Identity
        [InlineData("You are not a doctor, a nurse or a therapist")]
        [InlineData("Reply in the language the parent writes in")]
        [InlineData("no markdown")]
        [InlineData("Never be negative")]
        [InlineData("contact emergency services now")]
        [InlineData("not medical advice")]
        [InlineData("talking to their pediatrician")]
        [InlineData("Never diagnose")]
        [InlineData("Never give a medication name with a dose")]
        [InlineData("Never suggest delaying, skipping or replacing care")]
        [InlineData("Vaccines follow the official schedule and the baby's pediatrician")]
        [InlineData("Never judge growth from a single measurement")]
        [InlineData("Be kind about the parent's own wellbeing")]
        [InlineData("Don't judge parenting choices")]
        [InlineData("Decline off-topic requests graciously")]
        public void BuildSystemPrompt_AlwaysHoldsEachRule(string marker)
        {
            BabyModel baby = new() { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-2) };

            Assert.Contains(marker, CubGuidelines.BuildSystemPrompt(null));
            Assert.Contains(marker, CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [], Catalog)));
        }

        //"General advice" sends no baby data
        [Fact]
        public void BuildSystemPrompt_GeneralAdvice_HasNoBabyBlock()
        {
            string prompt = CubGuidelines.BuildSystemPrompt(null);

            Assert.Contains("\"General advice\"", prompt);
            Assert.Contains("\"your baby\" or \"the little one\"", prompt);
            Assert.DoesNotContain("The parent picked", prompt);
            Assert.DoesNotContain("- Age:", prompt);
            Assert.DoesNotContain("Vaccines recorded", prompt);
        }

        [Fact]
        public void BuildSystemPrompt_PickedBaby_AddsEachContextLine()
        {
            BabyModel baby = new() { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-4), HeightInFt = 2.1, WeightInLbs = 13 };
            VaccineModel vaccine = Catalog.Vaccines[0];
            var record = new VaccinationRecordModel { BabyId = baby.Id, VaccineId = vaccine.Id, TotalDoses = 3, CompletedDoses = 3 };

            string prompt = CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [record], Catalog));

            Assert.Contains("The parent picked their baby Lia for this chat. You may call the baby by name.", prompt);
            Assert.Contains("- Age: 4 months old", prompt);
            Assert.Contains("- Height: 2.1 ft (date unknown)", prompt);
            Assert.Contains("- Weight: 13 lbs (date unknown)", prompt);
            Assert.Contains($"- Vaccines recorded in the app:{Environment.NewLine}  - {vaccine.Name}: complete (3 of 3 doses)", prompt);
            Assert.DoesNotContain("\"General advice\"", prompt);
        }

        [Fact]
        public void BuildSystemPrompt_PickedBabyWithNoVaccines_SaysNoneRecorded()
        {
            BabyModel baby = new() { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-4) };

            string prompt = CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [], Catalog));

            Assert.EndsWith("- Vaccines: none recorded in the app", prompt);
        }

        [Fact]
        public void TrimHistory_UnderTheCap_KeepsEveryTurn()
        {
            ChatTurn[] turns = [Parent("hi"), Cub("hello"), Parent("tip?")];

            Assert.Equal(turns, CubGuidelines.TrimHistory(turns, 20));
        }

        [Fact]
        public void TrimHistory_OverTheCap_KeepsTheMostRecentTurns()
        {
            ChatTurn[] turns = Enumerable.Range(1, 25).Select(i => i % 2 == 1 ? Parent($"p{i}") : Cub($"c{i}")).ToArray();

            IReadOnlyList<ChatTurn> sent = CubGuidelines.TrimHistory(turns, 20);

            Assert.Equal(19, sent.Count); //The 20th-last turn is Cub's, so it is dropped too
            Assert.Equal(Parent("p7"), sent[0]);
            Assert.Equal(Parent("p25"), sent[^1]);
        }

        //The first turn sent is always the parent's
        [Fact]
        public void TrimHistory_CutLandsOnCubTurns_DropsThemFromTheStart()
        {
            ChatTurn[] turns = [Parent("p1"), Cub("c1"), Cub("c2"), Parent("p2")];

            Assert.Equal([Parent("p2")], CubGuidelines.TrimHistory(turns, 3));
        }

        [Fact]
        public void TrimHistory_ExactlyAtTheCap_KeepsEveryTurn()
        {
            ChatTurn[] turns = Enumerable.Range(1, 20).Select(i => i % 2 == 1 ? Parent($"p{i}") : Cub($"c{i}")).ToArray();

            Assert.Equal(turns, CubGuidelines.TrimHistory(turns, 20));
        }

        [Fact]
        public void TrimHistory_OnlyCubTurnsOrNone_IsEmpty()
        {
            Assert.Empty(CubGuidelines.TrimHistory([Cub("c1"), Cub("c2")], 20));
            Assert.Empty(CubGuidelines.TrimHistory([], 20));
        }

        [Fact]
        public void TrimHistory_DefaultCap_IsTwentyTurns()
        {
            ChatTurn[] turns = Enumerable.Range(1, 30).Select(i => Parent($"p{i}")).ToArray();

            Assert.Equal(CubGuidelines.MaxTurnsSent, CubGuidelines.TrimHistory(turns).Count);
            Assert.Equal(20, CubGuidelines.MaxTurnsSent);
            Assert.Equal(400, CubGuidelines.MaxReplyTokens);
        }
    }
}
