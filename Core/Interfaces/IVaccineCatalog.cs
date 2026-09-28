using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Core.Interfaces
{
    public interface IVaccineCatalog //Serves the fixed vaccine list and the schedule it was taken from; read-only, outside the database (ADR-012)
    {
        IReadOnlyList<VaccineModel> Vaccines { get; } //In display order
        VaccineModel? GetById(Guid vaccineId);

        string SourceName { get; }
        string SourceVersion { get; }
        Uri SourceUrl { get; }
        DateOnly LastReviewed { get; }
    }
}
