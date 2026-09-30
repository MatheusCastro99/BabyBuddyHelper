using System.Globalization;

namespace BabyBuddyHelper.UI.Converters
{
    //Resolves a baby's Id into that baby's dashboard vaccine tag ("Due soon: MMR"), so the card keeps binding to BabyModel.
    //Expects values[0] = the baby's Id (Guid) and values[1] = a tag lookup (IReadOnlyDictionary<Guid, string>).
    //Bound to text it returns the tag, or null when nothing is due; bound to IsVisible it returns whether a tag exists.
    internal class VaccineTagConverter : IMultiValueConverter
    {
        public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            string? vaccineTag = values.Length >= 2
                && values[0] is Guid babyId
                && values[1] is IReadOnlyDictionary<Guid, string> vaccineTagsByBabyId
                    ? vaccineTagsByBabyId.GetValueOrDefault(babyId)
                    : null;

            return targetType == typeof(bool) ? vaccineTag is not null : vaccineTag;
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
