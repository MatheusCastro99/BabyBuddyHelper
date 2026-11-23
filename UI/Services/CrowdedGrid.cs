namespace BabyBuddyHelper.UI.Services;

//Lays out a two-cell form row. Side by side (two equal columns) while each cell's natural width fits its half; stacked, one above the
//other, when either wouldn't (large text, or a narrow phone), so a date, a time or a placeholder is never cut off.
//The choice comes from the grid's width and the cells' natural widths, never from the current layout, so it can't flip back and forth
public static class CrowdedGrid
{
    public static void LayOut(Grid grid)
    {
        if (grid.Width <= 0)
        {
            return;
        }

        View[] cells = [.. grid.Children.OfType<View>()];
        double half = (grid.Width - grid.ColumnSpacing) / 2;
        bool stacked = cells.Any(cell => cell.Measure(double.PositiveInfinity, double.PositiveInfinity).Width > half);

        grid.ColumnDefinitions = stacked
            ? [new ColumnDefinition(GridLength.Star)]
            : [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)];
        grid.RowDefinitions = stacked
            ? [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)]
            : [new RowDefinition(GridLength.Auto)];
        grid.RowSpacing = stacked ? grid.ColumnSpacing : 0;

        for (int i = 0; i < cells.Length; i++)
        {
            Grid.SetColumn(cells[i], stacked ? 0 : i);
            Grid.SetRow(cells[i], stacked ? i : 0);
        }
    }
}
