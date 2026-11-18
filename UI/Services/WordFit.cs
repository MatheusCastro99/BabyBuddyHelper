using System.Runtime.CompilerServices;

namespace BabyBuddyHelper.UI.Services;

//For rows that move something aside when a word of their text wouldn't fit: a label that wraps only breaks between words, so a word
//wider than its column breaks mid-word. This measures the widest word of a label, at the label's own font, so the row can decide
//from the widths alone (never from its current layout, so the decision can't flip back and forth)
public static class WordFit
{
    private static readonly ConditionalWeakTable<Label, Label> Measurers = new();

    //A label needs a real handler to be measured, so a hidden one of the same font is added to the host grid, in its first cell:
    //one word on one line, never taller than the label it copies. The host is the grid that holds the source label
    public static double WidestWord(Label source, Grid host)
    {
        if (!Measurers.TryGetValue(source, out Label? measurer))
        {
            measurer = new Label
            {
                Opacity = 0,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.NoWrap
            };
            AutomationProperties.SetIsInAccessibleTree(measurer, false);
            host.Children.Add(measurer);
            Measurers.Add(source, measurer);
        }

        measurer.FontFamily = source.FontFamily;
        measurer.FontSize = source.FontSize;
        measurer.FontAttributes = source.FontAttributes;
        measurer.Text = (source.Text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).MaxBy(word => word.Length) ?? string.Empty;

        return measurer.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
    }
}
