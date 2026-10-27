namespace BabyBuddyHelper.Core.Models.Cub
{
    //What the chat shows for one message (#97, ADR-020): a reply that passed every check, or nothing, when both tries failed.
    public abstract record CubOutcome
    {
        private CubOutcome()
        {
        }

        public sealed record Reply(string Text) : CubOutcome;

        public sealed record Declined() : CubOutcome;
    }
}
