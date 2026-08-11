using BabyBuddyHelper.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BabyBuddyHelper.Pages;

public partial class ChecklistPage : ContentPage
{
    
    public ObservableCollection<TaskModel> TaskList { get; set; }
    List<TaskModel> tasks = new();
    public ICommand DeleteTaskCommand { get; } //Binds DeleteTaskCommand on .xaml to DeleteTask method in this file.
    public ICommand EditTaskCommand { get; }

    public bool isPendingFirst {  get; set; } = false;
    public ChecklistPage()
	{
		InitializeComponent();

        TaskList = new();
        DeleteTaskCommand = new Command<TaskModel>(DeleteTask);
        EditTaskCommand = new Command<TaskModel>(EditTask);
        
        tasks.Add(new TaskModel(5, "Organize Room", "Make Space for the baby!"));
        tasks.Add(new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!"));
        tasks.Add(new AppointmentModel("NJ", new(2026, 09, 15), 7, "BabyShower", "Get gifts"));
        tasks.Add(new AppointmentModel("Hospotal", new(2026, 09, 15), 8, "Imaging", "See the baby!"));

        OrganizeByPriority(tasks);

        BindingContext = this;
    }

    public void OrganizeByPriority(List<TaskModel> currentList)
    {
        var sorted = currentList.OrderByDescending(t => t.TaskPriority).ToList();
        TaskList.Clear();
        foreach (var item in sorted)
        {
            TaskList.Add(item);
        }
    }

    public void OrganizeByPending(List<TaskModel> currentList)
    {
        var sorted = currentList
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.TaskPriority)
            .ToList();

        TaskList.Clear();
        foreach (var item in sorted)
        {
            TaskList.Add(item);
        }
    }

    public void IsPendingFirstHandler(object sender, ToggledEventArgs e)
    {
        if (isPendingFirst)
        {
            OrganizeByPending(tasks);
        }
        else 
        {
            OrganizeByPriority(tasks);
        }
    }

    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(tasks, RefreshTaskList));
        //Debug.WriteLine("Checklist Clicked");
    }

    private void DeleteTask(TaskModel taskToDelete)
    {
        if (taskToDelete != null)
        {
            tasks.Remove(taskToDelete);
            TaskList.Remove(taskToDelete);
        }

        RefreshTaskList();
    }

    private async void EditTask(TaskModel taskToEdit)
    {
        if(taskToEdit != null)
        {
            await Navigation.PushModalAsync(new AddTaskPage(tasks, taskToEdit, RefreshTaskList));
        }
    }

    private void RefreshTaskList()
    {
        OrganizeByPriority(tasks);
    }
}