using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Core.Interfaces
{
    public interface IVaccineCatalog //Serves the fixed vaccine list and the schedule it was taken from; read-only, outside the database (ADR-012)
    {
        IReadOnlyList<VaccineModel> Vaccines { get; } //Routine vaccines, in display order
        IReadOnlyList<VaccineModel> RecurrentVaccines { get; } //Seasonal vaccines (flu, COVID-19), in display order
        VaccineModel? GetById(Guid vaccineId); //Looks in both lists

        string SourceName { get; }
        string SourceVersion { get; }
        Uri SourceUrl { get; }
        DateOnly LastReviewed { get; }
    }
}
