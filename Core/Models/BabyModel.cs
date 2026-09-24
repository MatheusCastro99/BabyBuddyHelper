namespace BabyBuddyHelper.Models
{
    public class BabyModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public double WeightInLbs { get; set; }
        public double HeightInFt { get; set; }
        public DateTime LastFeed { get; set; }
        public DateTime LastSleep { get; set; }

        //Computed for display only, never stored, so the age can't go stale as the baby grows.
        //A future DateOfBirth is an expected baby, shown as a countdown to the due date.
        public string AgeText
        {
            get
            {
                DateTime dateOfBirth = DateOfBirth.Date;
                DateTime today = DateTime.Today;

                if (dateOfBirth == today)
                    return "Born today";

                return dateOfBirth > today
                    ? $"Due in {FormatSpan(today, dateOfBirth)}"
                    : $"{FormatSpan(dateOfBirth, today)} old";
            }
        }

        //Days under 1 month, months under 2 years, then years and months
        private static string FormatSpan(DateTime from, DateTime to)
        {
            int totalMonths = (to.Year - from.Year) * 12 + to.Month - from.Month;

            if (from.AddMonths(totalMonths) > to) //The month anniversary hasn't come around yet
                totalMonths--;

            if (totalMonths < 1)
                return Pluralize((to - from).Days, "day");

            if (totalMonths < 24)
                return Pluralize(totalMonths, "month");

            int years = totalMonths / 12;
            int months = totalMonths % 12;

            return months == 0
                ? Pluralize(years, "year")
                : $"{Pluralize(years, "year")}, {Pluralize(months, "month")}";
        }

        private static string Pluralize(int count, string unit) => count == 1 ? $"1 {unit}" : $"{count} {unit}s";
    }
}
