using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BabyBuddyHelper.Pages;

public partial class ChecklistPage : ContentPage
{

    // public ObservableCollection<TaskModel> TaskList { get; set; } //Will hold instances of TaskModel and AppointmentModel
    //List<TaskModel> tasks = new();
    private readonly ITaskListService _taskListService; //Dependency Injection for TaskListService
    public ObservableCollection<TaskModel> TaskList => _taskListService.Tasks; //Will hold instances of TaskModel and AppointmentModel
    //ICommand binding for delete and edit buttons on each task card
    public ICommand DeleteTaskCommand { get; }
    public ICommand EditTaskCommand { get; }

    public bool isPendingFirst { get; set; } = false; //Property bound to the PendingFirst switch on .xaml
    public ChecklistPage(ITaskListService taskListService)
    {
        InitializeComponent();

        //Initializes TaskList and Commands
        _taskListService = taskListService;
        DeleteTaskCommand = new Command<TaskModel>(DeleteTask);
        EditTaskCommand = new Command<TaskModel>(EditTask);

        //Some initial Mock Data
        _taskListService.Tasks.Add(new TaskModel(5, "Organize Room", "Make Space for the baby!"));
        _taskListService.Tasks.Add(new TaskModel(10, "Prepare for baby", "Baby about to go Hello World!"));
        _taskListService.Tasks.Add(new AppointmentModel("NJ", new(2026, 09, 15), new(23, 0, 0), new(23, 30, 0), 7, "BabyShower", "Get gifts"));
        _taskListService.Tasks.Add(new AppointmentModel("Hospotal", new(2026, 09, 25), new(09, 15, 0), new(10, 0, 0), 8, "Imaging", "See the baby!"));

        BindingContext = this;
    }

    //Organizes List by priority Property of each task
    //public void OrganizeByPriority(List<TaskModel> currentList)
    //{
    //    var sorted = currentList.OrderByDescending(t => t.TaskPriority).ToList();
    //    TaskList.Clear();
    //    foreach (var item in sorted)
    //    {
    //        TaskList.Add(item);
    //    }
    //}

    ////Filters complete tasks to the end of the list
    //public void OrganizeByPending(List<TaskModel> currentList)
    //{
    //    var sorted = currentList
    //        .OrderBy(t => t.IsCompleted)
    //        .ThenByDescending(t => t.TaskPriority)
    //        .ToList();

    //    TaskList.Clear();
    //    foreach (var item in sorted)
    //    {
    //        TaskList.Add(item);
    //    }
    //}

    //Method bound to the IsPendingFirstSwitch
    public void IsPendingFirstHandler(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            _taskListService.OrganizeByPending();
        }
        else
        {
            _taskListService.OrganizeByPriority();
        }
    }

    //Method Bound to NewTask button on .xaml
    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService));
    }

    private void DeleteTask(TaskModel taskToDelete)
    {
        if (taskToDelete != null)
        {
            _taskListService.Remove(taskToDelete);
        }
    }

    private async void EditTask(TaskModel taskToEdit)
    {
        if (taskToEdit is null) return;

        await Navigation.PushAsync(
            new AddTaskPage(_taskListService, taskToEdit));

    }
}

    //method called on edit, delete, and add new task to refresh list and display current tasks
//    private void RefreshTaskList()
//    {
//        OrganizeByPriority(tasks);
//    }
//}