using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Models.Vaccine;
using BabyBuddyHelper.Core.Services.Cub;
using BabyBuddyHelper.Core.Services.Vaccine;

namespace BabyBuddyHelper.Tests.Services.Cub
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
        [InlineData("In an emergency, the health and safety rules come before the style and length.")] //#97: no style delays it
        [InlineData("not medical advice")]
        [InlineData("talking to their pediatrician")]
        [InlineData("Never diagnose")]
        [InlineData("Never give a medication name with a dose")]
        [InlineData("Never suggest delaying, skipping or replacing care")]
        [InlineData("Vaccines follow the official schedule and the baby's pediatrician")]
        [InlineData("Never judge growth from a single measurement")]
        [InlineData("Be kind about the parent's own wellbeing")]
        [InlineData("contact emergency services or a crisis line now")] //#92: checklist rows 20–21 rely on it
        [InlineData("Don't judge parenting choices")]
        [InlineData("Decline off-topic requests graciously")]
        public void BuildSystemPrompt_EveryStyleAndLength_HoldsEachRule(string marker)
        {
            BabyModel baby = new() { Name = "Lia", DateOfBirth = DateTime.Today.AddMonths(-2) };

            foreach (CubPersonality personality in AllPersonalities)
            {
                Assert.Contains(marker, CubGuidelines.BuildSystemPrompt(null, personality));
                Assert.Contains(marker, CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [], Catalog), personality));
            }
        }

        public static readonly CubPersonality[] AllPersonalities =
            [.. Enum.GetValues<CubStyle>().SelectMany(style => Enum.GetValues<CubVerbosity>().Select(verbosity => new CubPersonality(style, verbosity)))];

        [Theory]
        [InlineData(CubStyle.Supportive, "Be warm and encouraging. Acknowledge how the parent feels")]
        [InlineData(CubStyle.Neutral, "Be calm and matter-of-fact. Answer plainly")]
        [InlineData(CubStyle.Direct, "Get straight to the point. Lead with the answer")]
        public void BuildSystemPrompt_Style_HasItsLineOnly(CubStyle style, string marker)
        {
            foreach (CubVerbosity verbosity in Enum.GetValues<CubVerbosity>())
            {
                string prompt = CubGuidelines.BuildSystemPrompt(null, new CubPersonality(style, verbosity));

                Assert.Contains($"- {CubGuidelines.StyleLine(style)}", prompt);
                Assert.Contains(marker, prompt);

                foreach (CubStyle other in Enum.GetValues<CubStyle>().Where(other => other != style))
                    Assert.DoesNotContain(CubGuidelines.StyleLine(other), prompt);
            }
        }

        [Theory]
        [InlineData(CubVerbosity.Short, "Answer in one or two sentences.")]
        [InlineData(CubVerbosity.Balanced, "Answer in a few clear sentences.")]
        [InlineData(CubVerbosity.Detailed, "Give a fuller answer, at most two short paragraphs.")]
        public void BuildSystemPrompt_Length_HasItsLineOnly(CubVerbosity verbosity, string line)
        {
            foreach (CubStyle style in Enum.GetValues<CubStyle>())
            {
                string prompt = CubGuidelines.BuildSystemPrompt(null, new CubPersonality(style, verbosity));

                Assert.Contains($"- {line}", prompt);

                foreach (CubVerbosity other in Enum.GetValues<CubVerbosity>().Where(other => other != verbosity))
                    Assert.DoesNotContain(CubGuidelines.LengthLine(other), prompt);
            }
        }

        //Today's fixed "warm" and "short" lines would contradict Neutral, Direct and Detailed, so only the chosen lines set them
        [Theory]
        [InlineData(CubStyle.Neutral)]
        [InlineData(CubStyle.Direct)]
        public void BuildSystemPrompt_NotSupportive_NeverAsksForWarmth(CubStyle style)
        {
            string prompt = CubGuidelines.BuildSystemPrompt(null, new CubPersonality(style, CubVerbosity.Detailed));

            Assert.DoesNotContain("warm", prompt, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Keep replies short", prompt);
        }

        [Theory]
        [InlineData(CubVerbosity.Short, 250)]
        [InlineData(CubVerbosity.Balanced, 400)]
        [InlineData(CubVerbosity.Detailed, 700)]
        public void MaxReplyTokensFor_EachLength_HasItsCap(CubVerbosity verbosity, int cap)
        {
            Assert.Equal(cap, CubGuidelines.MaxReplyTokensFor(verbosity));
        }

        [Fact]
        public void Default_IsSupportiveAndBalanced()
        {
            Assert.Equal(new CubPersonality(CubStyle.Supportive, CubVerbosity.Balanced), CubPersonality.Default);
        }

        //"General advice" sends no baby data
        [Fact]
        public void BuildSystemPrompt_GeneralAdvice_HasNoBabyBlock()
        {
            string prompt = CubGuidelines.BuildSystemPrompt(null, CubPersonality.Default);

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

            string prompt = CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [record], Catalog), CubPersonality.Default);

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

            string prompt = CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [], Catalog), CubPersonality.Default);

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
        }
    }
}
