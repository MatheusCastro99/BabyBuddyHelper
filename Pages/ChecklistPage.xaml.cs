using BabyBuddyHelper.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BabyBuddyHelper.Pages;

public partial class ChecklistPage : ContentPage
{
    
    public ObservableCollection<TaskModel> TaskList { get; set; } //Will hold instances of TaskModel and AppointmentModel
    List<TaskModel> tasks = new();
    public ICommand DeleteTaskCommand { get; } //Binds DeleteTaskCommand on .xaml to DeleteTask method in this file.
    public ICommand EditTaskCommand { get; } //Binds EditTaskCommand on .xaml to DeleteTask method in this file.

    public bool isPendingFirst {  get; set; } = false; //Property bound to the PendingFirst switch on .xaml
    public ChecklistPage()
	{
		InitializeComponent();

        //Initializes TaskList and Commands
        TaskList = new();
        DeleteTaskCommand = new Command<TaskModel>(DeleteTask);
        EditTaskCommand = new Command<TaskModel>(EditTask);
        
        //Some initial Mock Data
        tasks.Add(new TaskModel(5, "Organize Room", "Make Space for the baby!"));
        tasks.Add(new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!"));
        tasks.Add(new AppointmentModel("NJ", new(2026, 09, 15), 7, "BabyShower", "Get gifts"));
        tasks.Add(new AppointmentModel("Hospotal", new(2026, 09, 15), 8, "Imaging", "See the baby!"));

        OrganizeByPriority(tasks);

        BindingContext = this;
    }

    //Organizes List by priority Property of each task
    public void OrganizeByPriority(List<TaskModel> currentList)
    {
        var sorted = currentList.OrderByDescending(t => t.TaskPriority).ToList();
        TaskList.Clear();
        foreach (var item in sorted)
        {
            TaskList.Add(item);
        }
    }

    //Filters complete tasks to the end of the list
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

    //Method bound to the IsPendingFirstSwitch
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

    //Method Bound to NewTask button on .xaml
    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(tasks, RefreshTaskList));
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
            if(taskToEdit is AppointmentModel appointmentToEdit)
            {
                await Navigation.PushModalAsync(new AddTaskPage(tasks, appointmentToEdit, RefreshTaskList));
                return;
            }

            await Navigation.PushModalAsync(new AddTaskPage(tasks, taskToEdit, RefreshTaskList));
        }
    }

    //method called on edit, delete, and add new task to refresh list and display current tasks
    private void RefreshTaskList()
    {
        OrganizeByPriority(tasks);
    }
}