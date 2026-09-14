namespace BabyBuddyHelper.Models
{
    public class BabyModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double WeightInLbs { get; set; }
        public double HeightInFt { get; set; }
        public DateTime LastFeed { get; set; }
        public DateTime LastSleep { get; set; }
    }
}
