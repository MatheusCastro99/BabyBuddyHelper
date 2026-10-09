using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //Sample records with sensible defaults, so each test only spells out the values it is about
    public static class TestData
    {
        public static TaskModel Task(string name = "Buy crib", int priority = 2, Guid? babyId = null) =>
            new(priority, name, "Check the safety rating") { AssociatedBabyId = babyId };

        public static AppointmentModel Appointment(string name = "Pediatrician visit", int priority = 3, Guid? babyId = null) =>
            new("Riverside Clinic", DateTime.Today.AddDays(7), new TimeSpan(9, 30, 0), new TimeSpan(10, 15, 0), priority, name, "Two-month checkup")
            {
                AssociatedBabyId = babyId
            };

        public static BabyModel Baby(string name = "Olivia") =>
            new() { Name = name, DateOfBirth = DateTime.Today.AddMonths(-3) };

        public static VaccinationRecordModel Record(Guid babyId, Guid? vaccineId = null) =>
            new() { BabyId = babyId, VaccineId = vaccineId ?? Guid.NewGuid(), TotalDoses = 3, CompletedDoses = 1 };
    }
}
