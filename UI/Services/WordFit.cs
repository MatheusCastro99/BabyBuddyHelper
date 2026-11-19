using System.Runtime.CompilerServices;

namespace BabyBuddyHelper.UI.Services;

//For rows that move something aside when a word of their text wouldn't fit: a label that wraps only breaks between words, so a word
//wider than its column breaks mid-word. This measures the widest word of a label or a button, at its own font, so the row can decide
//from the widths alone (never from its current layout, so the decision can't flip back and forth)
public static class WordFit
{
    private static readonly ConditionalWeakTable<VisualElement, Label> Measurers = new();

    //A label needs a real handler to be measured, so a hidden one of the same font is added to the host grid: one word on one line,
    //never taller than the text it copies. The host is the grid that holds the source. The measurer sits in the given column, which
    //must be a star column (an Auto column would grow to the width of the word)
    public static double WidestWord(Label source, Grid host, int column = 0) =>
        WidestWord(source, source.Text, source.FontFamily, source.FontSize, source.FontAttributes, host, column);

    public static double WidestWord(Button source, Grid host, int column = 0) =>
        WidestWord(source, source.Text, source.FontFamily, source.FontSize, source.FontAttributes, host, column);

    private static double WidestWord(VisualElement source, string? text, string? fontFamily, double fontSize, FontAttributes fontAttributes, Grid host, int column)
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
            Grid.SetColumn(measurer, column);
            host.Children.Add(measurer);
            Measurers.Add(source, measurer);
        }

        measurer.FontFamily = fontFamily;
        measurer.FontSize = fontSize;
        measurer.FontAttributes = fontAttributes;
        measurer.Text = (text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).MaxBy(word => word.Length) ?? string.Empty;

        return measurer.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
    }
}
