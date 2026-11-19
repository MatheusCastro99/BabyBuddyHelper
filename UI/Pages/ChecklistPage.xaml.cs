using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces.Baby;
using BabyBuddyHelper.Core.Interfaces.TaskList;
using BabyBuddyHelper.Core.Models.TaskList;
using BabyBuddyHelper.UI.Controls;
using BabyBuddyHelper.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BabyBuddyHelper.UI.Pages;

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

        KeyboardFocusRing.Attach(PendingFirstSwitch, PendingFirstFocusRing);
        KeyboardFocusRing.Attach(DateOrderSwitch, DateOrderFocusRing);

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

        //The trash icon sits right next to the edit icon, so a delete is confirmed first, same as deleting a baby profile
        bool isAppointment = taskToDelete is AppointmentModel;
        bool shouldDelete = await DisplayAlertAsync(
            isAppointment ? "Delete appointment?" : "Delete task?",
            isAppointment ? $"Remove {taskToDelete.TaskName} from your list and calendar?" : $"Remove {taskToDelete.TaskName} from your list?",
            "Delete",
            "Keep");

        if (!shouldDelete)
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

    //A task card reads: the text on the left, the done box and the edit and delete buttons on the right. On a narrow card (a phone) the
    //buttons beside the text would leave it a sliver, so edit and delete move to the priority chip row, on the right, and only the done
    //box stays beside the text. And when a word of the text (name or description) wouldn't fit beside what is left next to it (large
    //text), it would break mid-word, so the text takes the full width and that goes on its own row under it, to the right.
    //The grid keeps its three columns and three rows for good: only where the cells sit changes, so an unused row is empty and flat
    private void OnTaskRowSizeChanged(object? sender, EventArgs e)
    {
        if (sender is Grid row)
        {
            row.Dispatcher.Dispatch(() => LayOutTaskRow(row)); //Not inside the size change itself: see ChatPage.OnCubCardSizeChanged
        }
    }

    //A recycled card keeps its size when it is bound to another task, so no SizeChanged follows: lay it out again for the new text.
    //Dispatched, so the labels have their new text by then
    private void OnTaskRowBindingContextChanged(object? sender, EventArgs e)
    {
        if (sender is Grid row)
        {
            row.Dispatcher.Dispatch(() => LayOutTaskRow(row));
        }
    }

    private static void LayOutTaskRow(Grid row)
    {
        if (row.Width <= 0)
        {
            return;
        }

        VerticalStackLayout text = row.Children.OfType<VerticalStackLayout>().First();
        CheckBox done = row.Children.OfType<CheckBox>().First();
        Grid editDelete = row.Children.OfType<Grid>().First();
        Border chip = row.Children.OfType<Border>().First();
        Label name = ((FlexLayout)text.Children[0]).Children.OfType<Label>().First();
        Label description = text.Children.OfType<Label>().First();

        const double SideBySideSpacing = 12; //Between the text and what sits beside it
        const double MinTitleWidth = 280; //Narrower than this with all three beside it, and edit and delete go to the chip row
        const double ChipRowButtonSpacing = 8;
        const double StackedTopMargin = 4; //Between the text and the buttons under it
        const double ChipTopMargin = 10;

        double doneWidth = done.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        editDelete.ColumnSpacing = 0; //Measured without the chip row's gap, so the same width decides every time
        double editDeleteWidth = editDelete.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        bool narrow = row.Width - doneWidth - editDeleteWidth - SideBySideSpacing < MinTitleWidth;

        //Narrow: the edit and delete pair sits on the chip row, so only the done box is beside the text
        double besideWidth = narrow ? doneWidth : doneWidth + editDeleteWidth;
        double wordWidth = Math.Max(WordFit.WidestWord(name, row), WordFit.WidestWord(description, row));
        bool stacked = wordWidth > row.Width - besideWidth - SideBySideSpacing;

        text.Margin = new Thickness(0, 0, stacked ? 0 : SideBySideSpacing, 0);
        Grid.SetColumnSpan(text, stacked ? 3 : 1);

        //The done box: beside the text, or under it to the right. Under the text on a wide card, it sits left of the edit and delete buttons
        Grid.SetRow(done, stacked ? 1 : 0);
        Grid.SetColumn(done, stacked && narrow ? 0 : 1);
        Grid.SetColumnSpan(done, stacked && narrow ? 3 : 1);
        done.Margin = new Thickness(0, stacked ? StackedTopMargin : 0, 0, 0);

        //Edit and delete: beside the text (wide), under it with the done box (wide and stacked), or on the chip row (narrow)
        editDelete.ColumnSpacing = narrow ? ChipRowButtonSpacing : 0; //Wide: each button's 13px padding already spaces them
        if (narrow)
        {
            Grid.SetRow(editDelete, 2);
            Grid.SetColumn(editDelete, 0);
            Grid.SetColumnSpan(editDelete, 3); //Spans the star column, so the chip row's width never widens the done box's column
            editDelete.HorizontalOptions = LayoutOptions.End;
            editDelete.Margin = new Thickness(0, ChipTopMargin, 0, 0); //The same top margin as the chip, so the two stay centred on each other
        }
        else
        {
            Grid.SetRow(editDelete, stacked ? 1 : 0);
            Grid.SetColumn(editDelete, 2);
            Grid.SetColumnSpan(editDelete, 1);
            editDelete.HorizontalOptions = LayoutOptions.Fill;
            editDelete.Margin = new Thickness(0, stacked ? StackedTopMargin : 0, 0, 0);
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
