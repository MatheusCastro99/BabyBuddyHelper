using BabyBuddyHelper.Core.Interfaces.Vaccine;
using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Vaccine;
using System.Globalization;

namespace BabyBuddyHelper.Core.Models.Cub
{
    //What Cub may know about the baby the parent picked in the chat (#96), already written out as text.
    //It has no field for the birth date, the last feed or the last sleep, so those can never be sent (ADR-017).
    public sealed class BabyChatContext
    {
        public string Name { get; }
        public string Age { get; }
        public string Height { get; }
        public string Weight { get; }
        public IReadOnlyList<string> Vaccines { get; } //One line per recorded vaccine, empty when none is recorded

        private BabyChatContext(string name, string age, string height, string weight, IReadOnlyList<string> vaccines)
        {
            Name = name;
            Age = age;
            Height = height;
            Weight = weight;
            Vaccines = vaccines;
        }

        //Records for a vaccine the catalog doesn't know are left out: without a name they tell Cub nothing.
        public static BabyChatContext From(BabyModel baby, IEnumerable<VaccinationRecordModel> records, IVaccineCatalog catalog)
        {
            var vaccines = new List<string>();

            foreach (VaccinationRecordModel record in records)
            {
                if (catalog.GetById(record.VaccineId) is { } vaccine)
                    vaccines.Add($"{vaccine.Name}: {DescribeProgress(record)}");
            }

            return new BabyChatContext(
                baby.Name,
                baby.AgeText,
                DescribeMeasurement(baby.HeightInFt, "ft", baby.HeightUpdatedOn),
                DescribeMeasurement(baby.WeightInLbs, "lbs", baby.WeightUpdatedOn),
                vaccines);
        }

        //A value of 0 means it was never measured. A null date is a value saved before the dates existed.
        private static string DescribeMeasurement(double value, string unit, DateTime? updatedOn)
        {
            if (value == 0)
                return "not recorded";

            string date = updatedOn is { } day ? $"measured on {FormatDate(day)}" : "date unknown";

            return $"{value.ToString("0.##", CultureInfo.InvariantCulture)} {unit} ({date})";
        }

        private static string DescribeProgress(VaccinationRecordModel record)
        {
            if (record.TotalDoses is not { } totalDoses) //Seasonal: no total, never complete, and a next dose is optional
            {
                string lastDose = record.LastAdministered is { } lastAdministered ? $", last on {FormatDate(lastAdministered)}" : string.Empty;
                string current = record.IsCurrent ? ", current this season" : string.Empty;
                string nextDose = record.NextDose is null ? string.Empty : DescribeNextDose(record);

                return $"seasonal, {Pluralize(record.CompletedDoses, "dose")} given{lastDose}{current}{nextDose}";
            }

            if (record.IsCompleted)
                return $"complete ({record.CompletedDoses} of {totalDoses} doses)";

            return $"{record.CompletedDoses} of {totalDoses} doses given{DescribeNextDose(record)}";
        }

        private static string DescribeNextDose(VaccinationRecordModel record)
        {
            if (record.NextDose is not { } nextDose)
                return ", no next dose date set";

            if (record.IsOverdue)
                return $", next dose overdue since {FormatDate(nextDose)}";

            if (record.IsDueToday)
                return ", next dose due today";

            return $", next dose due on {FormatDate(nextDose)}";
        }

        //Written in English, whatever the device's culture: Cub reads it and answers in the parent's language
        private static string FormatDate(DateTime date) => date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);

        private static string Pluralize(int count, string unit) => count == 1 ? $"1 {unit}" : $"{count} {unit}s";
    }
}
