using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BabyBuddyHelper.Pages;

public partial class ChecklistPage : ContentPage
{
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

        BindingContext = this;
    }

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

        if (taskToEdit is AppointmentModel) //Checks task type to trigger right constructor on AddTaskPage
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, (AppointmentModel)taskToEdit));
            return;
        }
        else
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, taskToEdit));
        }
    }
}