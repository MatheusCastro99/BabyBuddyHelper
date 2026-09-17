namespace BabyBuddyHelper.Interfaces
{
    public interface IBabyFilterService //Centralizes the baby selection option lists shared by the checklist, calendar, and task entry screens.
    {
        Dictionary<string, Guid?> BuildOptions(string firstOptionLabel);

        //Returns the label for the given selection, falling back to firstOptionLabel when the baby no longer exists.
        (Guid? ResolvedBabyId, string Label) ResolveSelection(Guid? selectedBabyId, string firstOptionLabel);
    }
}
