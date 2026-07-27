using BabyPrepRegistry.Models;

namespace BabyPrepRegistry.Pages;

public partial class AddTaskPage : ContentPage
{
    List<TaskModel> currentTasks = new();
    Action? onTaskSaved;
	public AddTaskPage(List<TaskModel> currentTasks, Action? onTaskSaved = null)
	{
		InitializeComponent();

        this.currentTasks = currentTasks;
        this.onTaskSaved = onTaskSaved;
	}

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        int priority = Convert.ToInt32(PriorityStepper.Value);
        string taskName = TaskNameEntry.Text;
        string taskDescription = DescriptionEntry.Text;

        TaskModel newTask = new(priority, taskName, taskDescription);
        currentTasks.Add(newTask);

        onTaskSaved.Invoke();
        await Navigation.PopModalAsync();
    }
}