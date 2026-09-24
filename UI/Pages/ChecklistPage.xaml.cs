using BabyBuddyHelper.Exceptions;
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
    private readonly IBabyFilterService _babyFilterService;
    private Guid? _selectedBabyFilterId;
    private bool _isUpdatingOrderingSwitches;
    public ObservableCollection<TaskModel> TaskList { get; } = new(); //Will hold instances of TaskModel and AppointmentModel

    //Baby name lookup consumed by AssociatedBabyNameConverter. Replaced (not mutated) on every refresh so the
    //PropertyChanged notification re-evaluates each task card's baby label, including after a profile rename.
    public IReadOnlyDictionary<Guid, string> BabyNamesById { get; private set; } = new Dictionary<Guid, string>();

    //ICommand binding for delete and edit buttons on each task card
    public ICommand DeleteTaskCommand { get; }
    public ICommand EditTaskCommand { get; }

    public bool isPendingFirst { get; set; } = false; //Property bound to the PendingFirst switch on .xaml
    public bool isDateOrderEnabled { get; set; } = false;
    public ChecklistPage(ITaskListService taskListService, IBabyProfileService babyProfileService, IBabyFilterService babyFilterService)
    {
        InitializeComponent();

        //Initializes TaskList and Commands
        _taskListService = taskListService;
        _babyProfileService = babyProfileService;
        _babyFilterService = babyFilterService;
        DeleteTaskCommand = new Command<TaskModel>(async task => await DeleteTaskAsync(task));
        EditTaskCommand = new Command<TaskModel>(EditTask);
        _taskListService.Tasks.CollectionChanged += (_, _) => RefreshTaskList();
        _babyProfileService.BabyProfiles.CollectionChanged += (_, _) => RefreshTaskList();

        BindingContext = this;
        RefreshTaskList();
    }

    //Method bound to the IsPendingFirstSwitch
    public void IsPendingFirstHandler(object? sender, ToggledEventArgs e)
    {
        if (_isUpdatingOrderingSwitches)
        {
            return;
        }

        SetOrderingState(
            pendingFirstEnabled: e.Value,
            dateOrderEnabled: false);

        RefreshTaskList();
    }

    public void IsDateOrderHandler(object? sender, ToggledEventArgs e)
    {
        if (_isUpdatingOrderingSwitches)
        {
            return;
        }

        SetOrderingState(
            pendingFirstEnabled: false,
            dateOrderEnabled: e.Value);

        RefreshTaskList();
    }

    //Method Bound to NewTask button on .xaml
    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, _babyFilterService));
    }

    private async Task DeleteTaskAsync(TaskModel taskToDelete)
    {
        if (taskToDelete == null)
        {
            return;
        }

        try
        {
            await _taskListService.RemoveAsync(taskToDelete.Id);
            ToastService.Show(ToastKind.TaskDeleted);
        }
        catch (DbCommunicationException) //Nothing was removed, so the task is still on the list
        {
            await AlertService.ShowWriteFailedAsync(this, "Couldn't remove this task", "It's still on your list. Please try again in a moment.");
        }
    }

    //Persists completion through the service and shows companion-themed toast feedback when a task is completed.
    //The binding is OneWay, so the model only changes through the service. This event also fires when a row is
    //rendered; the model already matches the checkbox then, so only a real user tap gets past the first check.
    private async void OnTaskCompletedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox { BindingContext: TaskModel task } checkBox || task.IsCompleted == e.Value)
        {
            return;
        }

        try
        {
            await _taskListService.SetCompletionAsync(task.Id, e.Value);
        }
        catch (DbCommunicationException)
        {
            //The model never changed, so clearing the tap's value makes the checkbox fall back to its binding, which is the
            //stored state. The CheckedChanged it raises stops at the check above. Reset before the alert, so the corrected
            //state is already visible behind it. Never assign IsChecked here: a value set from code outranks the OneWay
            //binding for good, and the recycled checkbox would then show the wrong state for other tasks.
            checkBox.ClearValue(CheckBox.IsCheckedProperty);
            await AlertService.ShowWriteFailedAsync(this, "Couldn't update this task", "The checkbox is back to how it was. Please try again in a moment.");
            return;
        }

        if (e.Value)
        {
            ToastService.Show(ToastKind.TaskCompleted);
        }
    }

    private async void EditTask(TaskModel taskToEdit)
    {
        if (taskToEdit is null) return;

        if (taskToEdit is AppointmentModel appointmentToEdit) //Checks task type to trigger right constructor on AddTaskPage
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, _babyFilterService, appointmentToEdit));
        }
        else
        {
            await Navigation.PushModalAsync(
                new AddTaskPage(_taskListService, _babyFilterService, taskToEdit));
        }
    }

    private async void OnChecklistBabyFilterClicked(object? sender, EventArgs e)
    {
        Dictionary<string, Guid?> filterOptions = _babyFilterService.BuildOptions("All babies");

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
        RefreshSelectedBabyFilterLabel();

        BabyNamesById = _babyProfileService.BabyProfiles.ToDictionary(profile => profile.Id, profile => profile.Name);
        OnPropertyChanged(nameof(BabyNamesById));

        List<TaskModel> visibleTasks = _taskListService
            .GetTasks(_selectedBabyFilterId, isPendingFirst, isDateOrderEnabled)
            .ToList();

        TaskList.Clear();

        foreach (TaskModel task in visibleTasks)
        {
            TaskList.Add(task);
        }
    }

    private void RefreshSelectedBabyFilterLabel()
    {
        (Guid? resolvedBabyId, string label) = _babyFilterService.ResolveSelection(_selectedBabyFilterId, "All babies");

        _selectedBabyFilterId = resolvedBabyId;
        ChecklistBabyFilterButton.Text = label;
    }

    private void SetOrderingState(bool pendingFirstEnabled, bool dateOrderEnabled)
    {
        _isUpdatingOrderingSwitches = true;

        isPendingFirst = pendingFirstEnabled;
        isDateOrderEnabled = dateOrderEnabled;

        PendingFirstSwitch.IsToggled = isPendingFirst;
        DateOrderSwitch.IsToggled = isDateOrderEnabled;

        _isUpdatingOrderingSwitches = false;
    }
}
