using BabyBuddyHelper.Models;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    List<TaskModel> currentTasks = new();
    TaskModel? taskOnEdit;
    Action? onTaskSaved;
    bool isEditing = false; //false by default, meaning most of tasks are expected to be new tasks

	public AddTaskPage(List<TaskModel> currentTasks, Action? onTaskSaved = null) //Regular constructor called by New Task button
	{
		InitializeComponent();

        this.currentTasks = currentTasks;
        this.onTaskSaved = onTaskSaved;
	}

    public AddTaskPage(List<TaskModel> currentTasks, TaskModel taskOnEdit, Action? onTaskSaved = null) //Overload that will be triggered on EditNoteIcon click
    {
        InitializeComponent();

        this.currentTasks = currentTasks;
        this.onTaskSaved = onTaskSaved;
        this.taskOnEdit = taskOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked behavior

        TaskNameEntry.Text = taskOnEdit.TaskName; //Populate fields with taskOnEdit info
        DescriptionEntry.Text = taskOnEdit.TaskDescription;
        PriorityStepper.Value = taskOnEdit.TaskPriority;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if(!isEditing) //new task
        {
            int priority = Convert.ToInt32(PriorityStepper.Value);
            string taskName = TaskNameEntry.Text;
            string taskDescription = DescriptionEntry.Text;

            TaskModel newTask = new(priority, taskName, taskDescription);
            currentTasks.Add(newTask);

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }

        else //edited task
        {
            taskOnEdit?.TaskName = TaskNameEntry.Text;
            taskOnEdit?.TaskDescription = DescriptionEntry.Text;
            taskOnEdit?.TaskPriority = Convert.ToInt32(PriorityStepper.Value);

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }
        
    }
}