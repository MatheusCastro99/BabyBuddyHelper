namespace BabyBuddyHelper.Core.Models
{
    //One entry of the fixed vaccine catalog: reference data written into the code, never stored in the database (ADR-012)
    public class VaccineModel
    {
        public required Guid Id { get; init; } //Hardcoded and never regenerated, since vaccination records reference it
        public required string CvxCode { get; init; } //Kept as text so codes like "03" keep their leading zero
        public required string Name { get; init; }
        public required string Description { get; init; }
        public bool IsRecurrent { get; init; } //Given again each season (flu, COVID-19), so its records have no total and are never complete
    }
}
