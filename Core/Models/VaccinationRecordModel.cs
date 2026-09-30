namespace BabyBuddyHelper.Core.Models
{
    //One baby's progress on one catalog vaccine. At most one record per baby per vaccine (unique index in TrackerContext).
    //VaccineId points into the fixed catalog, which lives outside the database (ADR-012), so it has no foreign key.
    public class VaccinationRecordModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid BabyId { get; init; } //A record never moves to another baby or vaccine
        public required Guid VaccineId { get; init; }
        public int TotalDoses { get; set; } //Depends on the brand, so it's per record rather than in the catalog
        public int CompletedDoses { get; set; }
        public DateTime? LastAdministered { get; set; } //Empty until the first dose
        public DateTime? NextDose { get; set; } //Optional, and not needed once the record is complete

        //Computed for display only and never stored (get-only, so EF doesn't map them)
        public bool IsCompleted => CompletedDoses >= TotalDoses;

        //How far ahead a next dose counts as due soon (#81)
        public const int DueSoonWindowDays = 14;

        //A dose due today isn't overdue yet
        public bool IsOverdue => !IsCompleted && NextDose is { } nextDose && nextDose.Date < DateTime.Today;

        public bool IsDueToday => !IsCompleted && NextDose is { } nextDose && nextDose.Date == DateTime.Today;

        //Tomorrow through the window's last day. Today is "due today", not due soon
        public bool IsDueSoon => !IsCompleted && NextDose is { } nextDose
            && nextDose.Date > DateTime.Today && nextDose.Date <= DateTime.Today.AddDays(DueSoonWindowDays);
    }
}
