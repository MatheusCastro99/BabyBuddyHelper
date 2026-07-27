namespace BabyPrepRegistry.Pages;

public partial class AddTaskPage : ContentPage
{
	public AddTaskPage()
	{
		InitializeComponent();
	}

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Save your task logic here

        await Navigation.PopModalAsync();
    }
}