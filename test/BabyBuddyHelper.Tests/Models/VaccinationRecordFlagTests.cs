using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Tests.Models
{
    //The flags a vaccination record computes from its doses and dates. They read DateTime.Today, so every date here is
    //given as a number of days from today.
    public class VaccinationRecordFlagTests
    {
        private static DateTime DaysFromToday(int days) => DateTime.Today.AddDays(days);

        //A multi-dose record that still has doses left
        private static VaccinationRecordModel Pending(DateTime? nextDose) =>
            new() { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = 3, CompletedDoses = 1, NextDose = nextDose };

        private static VaccinationRecordModel Completed(DateTime? nextDose) =>
            new() { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = 3, CompletedDoses = 3, NextDose = nextDose };

        //A seasonal record: no dose total (flu, COVID)
        private static VaccinationRecordModel Seasonal(int completedDoses, DateTime? lastAdministered, DateTime? nextDose = null) =>
            new() { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = null, CompletedDoses = completedDoses, LastAdministered = lastAdministered, NextDose = nextDose };

        [Theory]
        [InlineData(3, 2, false)]
        [InlineData(3, 3, true)]
        [InlineData(3, 4, true)]
        [InlineData(1, 0, false)]
        public void IsCompleted_WithDoseTotal_IsTrueOnceAllDosesAreGiven(int totalDoses, int completedDoses, bool expected)
        {
            var record = new VaccinationRecordModel { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = totalDoses, CompletedDoses = completedDoses };

            Assert.Equal(expected, record.IsCompleted);
        }

        [Fact]
        public void IsCompleted_SeasonalRecord_IsNeverTrue()
        {
            Assert.False(Seasonal(completedDoses: 5, lastAdministered: DaysFromToday(-1)).IsCompleted);
        }

        [Theory]
        [InlineData(-30, true)]
        [InlineData(-1, true)]
        [InlineData(0, false)] //A dose due today isn't overdue yet
        [InlineData(1, false)]
        public void IsOverdue_PendingRecord_IsTrueOnlyForPastDates(int nextDoseInDays, bool expected)
        {
            Assert.Equal(expected, Pending(DaysFromToday(nextDoseInDays)).IsOverdue);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void IsDueToday_PendingRecord_IsTrueOnlyForToday(int nextDoseInDays, bool expected)
        {
            Assert.Equal(expected, Pending(DaysFromToday(nextDoseInDays)).IsDueToday);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)] //Today is "due today", not due soon
        [InlineData(1, true)]
        [InlineData(VaccinationRecordModel.DueSoonWindowDays, true)]
        [InlineData(VaccinationRecordModel.DueSoonWindowDays + 1, false)]
        public void IsDueSoon_PendingRecord_IsTrueFromTomorrowThroughTheWindow(int nextDoseInDays, bool expected)
        {
            Assert.Equal(expected, Pending(DaysFromToday(nextDoseInDays)).IsDueSoon);
        }

        //Each day falls under exactly one flag, or none once it is past the due-soon window
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(VaccinationRecordModel.DueSoonWindowDays)]
        public void DateFlags_PendingRecordInsideTheWindow_ExactlyOneIsTrue(int nextDoseInDays)
        {
            VaccinationRecordModel record = Pending(DaysFromToday(nextDoseInDays));

            Assert.Single(new[] { record.IsOverdue, record.IsDueToday, record.IsDueSoon }, flag => flag);
        }

        [Fact]
        public void DateFlags_NoNextDose_AreAllFalse()
        {
            VaccinationRecordModel record = Pending(nextDose: null);

            Assert.False(record.IsOverdue);
            Assert.False(record.IsDueToday);
            Assert.False(record.IsDueSoon);
        }

        //A finished record raises nothing, whatever date was left in NextDose
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void DateFlags_CompletedRecord_AreAllFalse(int nextDoseInDays)
        {
            VaccinationRecordModel record = Completed(DaysFromToday(nextDoseInDays));

            Assert.False(record.IsOverdue);
            Assert.False(record.IsDueToday);
            Assert.False(record.IsDueSoon);
        }

        //Only the day counts: a dose set for late tonight is due today, and one set for early tomorrow is not
        [Fact]
        public void DateFlags_TimeOfDay_IsIgnored()
        {
            VaccinationRecordModel lateToday = Pending(DateTime.Today.AddHours(23).AddMinutes(59));
            VaccinationRecordModel earlyTomorrow = Pending(DateTime.Today.AddDays(1).AddMinutes(1));

            Assert.True(lateToday.IsDueToday);
            Assert.False(lateToday.IsOverdue);
            Assert.True(earlyTomorrow.IsDueSoon);
            Assert.False(earlyTomorrow.IsDueToday);
        }

        [Fact]
        public void IsCurrent_SeasonalShotInsideTheWindow_IsTrue()
        {
            Assert.True(Seasonal(completedDoses: 1, lastAdministered: DaysFromToday(-1)).IsCurrent);
            Assert.True(Seasonal(completedDoses: 1, lastAdministered: DateTime.Today).IsCurrent);
        }

        //The window is 12 months back from today, and the day exactly 12 months ago is already outside it
        [Fact]
        public void IsCurrent_SeasonalShotAtTheWindowEdge_TurnsFalseAtTwelveMonths()
        {
            DateTime windowStart = DateTime.Today.AddMonths(-VaccinationRecordModel.CurrentWindowMonths);

            Assert.True(Seasonal(completedDoses: 1, lastAdministered: windowStart.AddDays(1)).IsCurrent);
            Assert.False(Seasonal(completedDoses: 1, lastAdministered: windowStart).IsCurrent);
            Assert.False(Seasonal(completedDoses: 1, lastAdministered: windowStart.AddDays(-1)).IsCurrent);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)] //Once the next dose is due, the record stops being current
        [InlineData(1, true)]
        public void IsCurrent_SeasonalShotWithNextDose_IsTrueOnlyWhileTheNextDoseIsAhead(int nextDoseInDays, bool expected)
        {
            VaccinationRecordModel record = Seasonal(completedDoses: 1, lastAdministered: DaysFromToday(-30), nextDose: DaysFromToday(nextDoseInDays));

            Assert.Equal(expected, record.IsCurrent);
        }

        [Fact]
        public void IsCurrent_SeasonalRecordWithNoShotYet_IsFalse()
        {
            Assert.False(Seasonal(completedDoses: 0, lastAdministered: null).IsCurrent);
            Assert.False(Seasonal(completedDoses: 0, lastAdministered: DaysFromToday(-1)).IsCurrent);
            Assert.False(Seasonal(completedDoses: 1, lastAdministered: null).IsCurrent);
        }

        //"Current" belongs to seasonal vaccines only. A record with a dose total is pending or completed instead.
        [Fact]
        public void IsCurrent_RecordWithDoseTotal_IsFalse()
        {
            var pending = new VaccinationRecordModel { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = 3, CompletedDoses = 1, LastAdministered = DaysFromToday(-1) };
            var completed = new VaccinationRecordModel { BabyId = Guid.NewGuid(), VaccineId = Guid.NewGuid(), TotalDoses = 3, CompletedDoses = 3, LastAdministered = DaysFromToday(-1) };

            Assert.False(pending.IsCurrent);
            Assert.False(completed.IsCurrent);
        }
    }
}
