using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Models.Vaccine;
using BabyBuddyHelper.Core.Services;
using System.Globalization;

namespace BabyBuddyHelper.Tests.Models
{
    //What Cub may know about a picked baby (#96). Ages and dose flags read DateTime.Today, so dates are days from today.
    public class BabyChatContextTests
    {
        private static readonly VaccineCatalog Catalog = new();
        private static readonly VaccineModel RoutineVaccine = Catalog.Vaccines[0];
        private static readonly VaccineModel SeasonalVaccine = Catalog.RecurrentVaccines[0];

        private static DateTime DaysFromToday(int days) => DateTime.Today.AddDays(days);

        private static string Format(DateTime date) => date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);

        private static BabyModel Baby() => new()
        {
            Name = "Lia",
            DateOfBirth = DateTime.Today.AddMonths(-4),
            HeightInFt = 2.1,
            HeightUpdatedOn = DaysFromToday(-40),
            WeightInLbs = 13.25,
            WeightUpdatedOn = DaysFromToday(-38),
            LastFeed = DaysFromToday(-1).AddHours(7).AddMinutes(45),
            LastSleep = DaysFromToday(-2).AddHours(21).AddMinutes(30)
        };

        private static VaccinationRecordModel Record(BabyModel baby, VaccineModel vaccine, int? totalDoses, int completedDoses,
            DateTime? lastAdministered = null, DateTime? nextDose = null) =>
            new() { BabyId = baby.Id, VaccineId = vaccine.Id, TotalDoses = totalDoses, CompletedDoses = completedDoses, LastAdministered = lastAdministered, NextDose = nextDose };

        [Fact]
        public void From_PickedBaby_SharesNameAgeAndDatedMeasurements()
        {
            BabyModel baby = Baby();

            BabyChatContext context = BabyChatContext.From(baby, [], Catalog);

            Assert.Equal("Lia", context.Name);
            Assert.Equal("4 months old", context.Age);
            Assert.Equal($"2.1 ft (measured on {Format(baby.HeightUpdatedOn!.Value)})", context.Height);
            Assert.Equal($"13.25 lbs (measured on {Format(baby.WeightUpdatedOn!.Value)})", context.Weight);
            Assert.Empty(context.Vaccines);
        }

        //A value saved before the dates existed (#95) has no date
        [Fact]
        public void From_MeasurementWithNoDate_SaysDateUnknown()
        {
            BabyModel baby = Baby();
            baby.HeightUpdatedOn = null;
            baby.WeightUpdatedOn = null;

            BabyChatContext context = BabyChatContext.From(baby, [], Catalog);

            Assert.Equal("2.1 ft (date unknown)", context.Height);
            Assert.Equal("13.25 lbs (date unknown)", context.Weight);
        }

        //0 means never measured, whatever date is left
        [Fact]
        public void From_MeasurementOfZero_SaysNotRecorded()
        {
            BabyModel baby = Baby();
            baby.HeightInFt = 0;
            baby.WeightInLbs = 0;

            BabyChatContext context = BabyChatContext.From(baby, [], Catalog);

            Assert.Equal("not recorded", context.Height);
            Assert.Equal("not recorded", context.Weight);
        }

        [Fact]
        public void From_ExpectedBaby_SharesTheDueDateCountdownAsTheAge()
        {
            var baby = new BabyModel { Name = "Bean", DateOfBirth = DateTime.Today.AddDays(10) };

            BabyChatContext context = BabyChatContext.From(baby, [], Catalog);

            Assert.Equal("Baby due in 10 days", context.Age);
            Assert.Equal("not recorded", context.Height);
            Assert.Equal("not recorded", context.Weight);
        }

        [Fact]
        public void From_RoutineVaccines_DescribeEachRecordsProgress()
        {
            BabyModel baby = Baby();
            VaccineModel second = Catalog.Vaccines[1];
            VaccineModel third = Catalog.Vaccines[2];
            VaccineModel fourth = Catalog.Vaccines[3];
            VaccineModel fifth = Catalog.Vaccines[4];
            DateTime nextDose = DaysFromToday(20);
            DateTime overdueDose = DaysFromToday(-3);

            BabyChatContext context = BabyChatContext.From(baby,
            [
                Record(baby, RoutineVaccine, totalDoses: 3, completedDoses: 3),
                Record(baby, second, totalDoses: 3, completedDoses: 1, nextDose: nextDose),
                Record(baby, third, totalDoses: 2, completedDoses: 1, nextDose: overdueDose),
                Record(baby, fourth, totalDoses: 4, completedDoses: 2, nextDose: DateTime.Today),
                Record(baby, fifth, totalDoses: 2, completedDoses: 0)
            ], Catalog);

            Assert.Equal(
            [
                $"{RoutineVaccine.Name}: complete (3 of 3 doses)",
                $"{second.Name}: 1 of 3 doses given, next dose due on {Format(nextDose)}",
                $"{third.Name}: 1 of 2 doses given, next dose overdue since {Format(overdueDose)}",
                $"{fourth.Name}: 2 of 4 doses given, next dose due today",
                $"{fifth.Name}: 0 of 2 doses given, no next dose date set"
            ], context.Vaccines);
        }

        [Fact]
        public void From_SeasonalVaccine_DescribesDosesLastShotAndSeason()
        {
            BabyModel baby = Baby();
            DateTime lastShot = DaysFromToday(-30);
            DateTime oldShot = DateTime.Today.AddMonths(-13);
            VaccineModel otherSeasonal = Catalog.RecurrentVaccines[1];

            BabyChatContext current = BabyChatContext.From(baby, [Record(baby, SeasonalVaccine, totalDoses: null, completedDoses: 1, lastAdministered: lastShot)], Catalog);
            BabyChatContext lapsed = BabyChatContext.From(baby, [Record(baby, otherSeasonal, totalDoses: null, completedDoses: 2, lastAdministered: oldShot)], Catalog);
            BabyChatContext none = BabyChatContext.From(baby, [Record(baby, SeasonalVaccine, totalDoses: null, completedDoses: 0)], Catalog);

            Assert.Equal($"{SeasonalVaccine.Name}: seasonal, 1 dose given, last on {Format(lastShot)}, current this season", Assert.Single(current.Vaccines));
            Assert.Equal($"{otherSeasonal.Name}: seasonal, 2 doses given, last on {Format(oldShot)}", Assert.Single(lapsed.Vaccines));
            Assert.Equal($"{SeasonalVaccine.Name}: seasonal, 0 doses given", Assert.Single(none.Vaccines));
        }

        [Fact]
        public void From_RecordForAVaccineTheCatalogDoesntKnow_IsLeftOut()
        {
            BabyModel baby = Baby();
            var unknown = new VaccinationRecordModel { BabyId = baby.Id, VaccineId = Guid.NewGuid(), TotalDoses = 2, CompletedDoses = 1 };

            BabyChatContext context = BabyChatContext.From(baby, [unknown, Record(baby, RoutineVaccine, totalDoses: 3, completedDoses: 3)], Catalog);

            Assert.Equal($"{RoutineVaccine.Name}: complete (3 of 3 doses)", Assert.Single(context.Vaccines));
        }

        //The birth date, last feed and last sleep never reach the context, so they can't reach the prompt
        [Fact]
        public void From_PickedBaby_NeverCarriesTheBirthDateFeedOrSleep()
        {
            BabyModel baby = Baby();

            string prompt = CubGuidelines.BuildSystemPrompt(BabyChatContext.From(baby, [Record(baby, RoutineVaccine, totalDoses: 3, completedDoses: 3)], Catalog), CubPersonality.Default);
            string babyBlock = prompt[prompt.IndexOf("The parent picked", StringComparison.Ordinal)..]; //The fixed rules mention feeding and sleep in general

            foreach (DateTime hidden in new[] { baby.DateOfBirth, baby.LastFeed, baby.LastSleep })
            {
                Assert.DoesNotContain(Format(hidden), prompt);
                Assert.DoesNotContain(hidden.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), prompt);
                Assert.DoesNotContain(hidden.ToString("MMM d", CultureInfo.InvariantCulture), prompt);
                Assert.DoesNotContain(hidden.ToString("h:mm", CultureInfo.InvariantCulture), prompt);
            }

            Assert.DoesNotContain("feed", babyBlock, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sleep", babyBlock, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("birth", babyBlock, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(["Name", "Age", "Height", "Weight", "Vaccines"], typeof(BabyChatContext).GetProperties().Select(property => property.Name));
        }
    }
}
