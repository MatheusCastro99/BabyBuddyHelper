using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BabyBuddyHelper.Pages;

public partial class ChecklistPage : ContentPage
{
    private readonly ITaskListService _taskListService; //Dependency Injection for TaskListService
    private readonly IBabyProfileService _babyProfileService;
    private readonly HashSet<Guid> _completedTaskIdsWithToast = new();
    private Guid? _selectedBabyFilterId;
    public ObservableCollection<TaskModel> TaskList { get; } = new(); //Will hold instances of TaskModel and AppointmentModel

    //ICommand binding for delete and edit buttons on each task card
    public ICommand DeleteTaskCommand { get; }
    public ICommand EditTaskCommand { get; }

    public bool isPendingFirst { get; set; } = false; //Property bound to the PendingFirst switch on .xaml
    public bool isDateOrderEnabled { get; set; } = false;
    public ChecklistPage(ITaskListService taskListService, IBabyProfileService babyProfileService)
    {
        InitializeComponent();

        //Initializes TaskList and Commands
        _taskListService = taskListService;
        _babyProfileService = babyProfileService;
        DeleteTaskCommand = new Command<TaskModel>(DeleteTask);
        EditTaskCommand = new Command<TaskModel>(EditTask);
        _taskListService.Tasks.CollectionChanged += (_, _) => RefreshTaskList();
        _babyProfileService.BabyProfiles.CollectionChanged += (_, _) => RefreshTaskList();

        BindingContext = this;
        RefreshTaskList();
        SyncCompletedTaskToastState();
    }

    //Method bound to the IsPendingFirstSwitch
    public void IsPendingFirstHandler(object sender, ToggledEventArgs e)
    {
        isPendingFirst = e.Value;
        RefreshTaskList();
    }

    public void IsDateOrderHandler(object sender, ToggledEventArgs e)
    {
        isDateOrderEnabled = e.Value;
        RefreshTaskList();
    }

    //Method Bound to NewTask button on .xaml
    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, _babyProfileService));
    }

    private void DeleteTask(TaskModel taskToDelete)
    {
        if (taskToDelete != null)
        {
            _taskListService.Remove(taskToDelete);
            ToastService.Show(ToastKind.TaskDeleted);
        }
    }

    //Shows companion-themed toast feedback whenever a task is marked as completed
    private void OnTaskCompletedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox { BindingContext: TaskModel task })
        {
            return;
        }

        if (e.Value)
        {
            if (_completedTaskIdsWithToast.Add(task.Id))
            {
                ToastService.Show(ToastKind.TaskCompleted);
            }
            return;
        }

        _completedTaskIdsWithToast.Remove(task.Id);
    }

    private async void EditTask(TaskModel taskToEdit)
    {
        if (taskToEdit is null) return;

        if (taskToEdit is AppointmentModel appointmentToEdit) //Checks task type to trigger right constructor on AddTaskPage
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, _babyProfileService, appointmentToEdit));
        }
        else
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, _babyProfileService, taskToEdit));
        }
    }

    private void SyncCompletedTaskToastState()
    {
        _completedTaskIdsWithToast.Clear();

        foreach (var task in _taskListService.Tasks.Where(task => task.IsCompleted))
        {
            _completedTaskIdsWithToast.Add(task.Id);
        }
    }

    private async void OnChecklistBabyFilterClicked(object? sender, EventArgs e)
    {
        Dictionary<string, Guid?> filterOptions = new()
        {
            ["All babies"] = null
        };

        foreach (BabyModel profile in _babyProfileService.BabyProfiles)
        {
            string optionLabel = profile.Name;
            int duplicateCounter = 2;

            while (filterOptions.ContainsKey(optionLabel))
            {
                optionLabel = $"{profile.Name} ({duplicateCounter})";
                duplicateCounter++;
            }

            filterOptions[optionLabel] = profile.Id;
        }

        string selectedOption = await DisplayActionSheetAsync("Filter checklist by baby", "Cancel", null, filterOptions.Keys.ToArray());

        if (string.IsNullOrEmpty(selectedOption) || selectedOption == "Cancel")
        {
            return;
        }

        _selectedBabyFilterId = filterOptions[selectedOption];
        ChecklistBabyFilterButton.Text = selectedOption;
        RefreshTaskList();
    }

    private void RefreshTaskList()
    {
        if (_selectedBabyFilterId.HasValue && !_babyProfileService.BabyProfiles.Any(profile => profile.Id == _selectedBabyFilterId.Value))
        {
            _selectedBabyFilterId = null;
            ChecklistBabyFilterButton.Text = "All babies";
        }

        List<TaskModel> visibleTasks = _taskListService
            .GetTasks(_selectedBabyFilterId, isPendingFirst, isDateOrderEnabled)
            .ToList();

        TaskList.Clear();

        foreach (TaskModel task in visibleTasks)
        {
            TaskList.Add(task);
        }
    }
}
