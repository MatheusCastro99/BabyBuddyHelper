using Syncfusion.Maui.Scheduler;


namespace BabyBuddyHelper.Pages;

public partial class CalendarPage : ContentPage
{
	private DateTime? SelectedDate { get; set; } = DateTime.Today;
	public CalendarPage()
	{
		InitializeComponent();
	}
}