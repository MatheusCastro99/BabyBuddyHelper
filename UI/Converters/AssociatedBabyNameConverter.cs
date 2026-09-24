using System.Globalization;

namespace BabyBuddyHelper.Resources.Styles
{
    //Resolves a task's AssociatedBabyId into the baby's display name at render time, so tasks only store the Id.
    //Expects values[0] = AssociatedBabyId (Guid?) and values[1] = a name lookup (IReadOnlyDictionary<Guid, string>).
    internal class AssociatedBabyNameConverter : IMultiValueConverter
    {
        private const string UnassignedLabel = "General";

        public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Length < 2
                || values[0] is not Guid associatedBabyId
                || values[1] is not IReadOnlyDictionary<Guid, string> babyNamesById)
            {
                return UnassignedLabel;
            }

            //A missing entry means the profile was deleted, so the task falls back to the unassigned label
            return babyNamesById.TryGetValue(associatedBabyId, out string? babyName) && !string.IsNullOrWhiteSpace(babyName)
                ? babyName
                : UnassignedLabel;
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
