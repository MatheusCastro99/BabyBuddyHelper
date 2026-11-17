using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces.Baby;
using BabyBuddyHelper.Core.Interfaces.TaskList;
using BabyBuddyHelper.Core.Models.TaskList;
using BabyBuddyHelper.UI.Services;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Diagnostics;

namespace BabyBuddyHelper.UI.Pages;

public partial class AddTaskPage : ContentPage
{
    private readonly ITaskListService _taskListService;
    private readonly IBabyFilterService _babyFilterService;
    private const int MinPriority = 1;
    private const int MaxPriority = 10;
    private const int DefaultPriority = 5;
    private int _priority = DefaultPriority;
    private Guid? _selectedAssociatedBabyId;
    TaskModel? taskOnEdit;
    AppointmentModel? appointmentOnEdit;

    private bool IsEditing => taskOnEdit is not null || appointmentOnEdit is not null;

    public AddTaskPage(ITaskListService taskListService, IBabyFilterService babyFilterService, DateTime? dateTime = null) //Regular constructor called by New Task
    {                                                                                                                                       //button on ChecklistPage or clicking an
        InitializeComponent();                                                                                                               //empty time cell on CalendarPage

        this._taskListService = taskListService;
        this._babyFilterService = babyFilterService;
        InitializeBabyProfilePicker();
        RefreshPriorityState();

        if (!(dateTime is null)) //Determines which Page modal is being called from.
        {                        //If it's from CalendarPage, fill information from time cell clicked
            IsAppointmentCheckBox.IsChecked = true;
            DateEntry.Date = dateTime;
            StartingTimeEntry.Time = dateTime.Value.TimeOfDay;
            EndingTimeEntry.Time = dateTime.Value.TimeOfDay.Add(new TimeSpan(01, 0, 0));
        }

        Debug.WriteLine("Creating New Task");
    }

    public AddTaskPage(ITaskListService taskListService, IBabyFilterService babyFilterService, TaskModel taskOnEdit) //Constructor that will be
    {                                                                                                                                  //triggered on EditNoteIcon click for regular tasks
        InitializeComponent();

        this._taskListService = taskListService;
        this._babyFilterService = babyFilterService;
        this.taskOnEdit = taskOnEdit;
        ApplyEditMode(taskOnEdit);

        Debug.WriteLine("Editing a regular task");
    }

    public AddTaskPage(ITaskListService taskListService, IBabyFilterService babyFilterService, AppointmentModel appointmentOnEdit) //Constructor that
    {                                                                                                                             //is triggered on EditNoteIcon click for appointments
        InitializeComponent();

        this._taskListService = taskListService;
        this._babyFilterService = babyFilterService;
        this.appointmentOnEdit = appointmentOnEdit;
        ApplyEditMode(appointmentOnEdit);

        IsAppointmentCheckBox.IsChecked = true;
        DateEntry.Date = appointmentOnEdit.AppointmentDate;
        StartingTimeEntry.Time = appointmentOnEdit.AppointmentStartTime;
        EndingTimeEntry.Time = appointmentOnEdit.AppointmentEndTime;
        LocationEntry.Text = appointmentOnEdit.AppointmentLocation;

        Debug.WriteLine("Editing an appointment");
    }

    private void ApplyEditMode(TaskModel taskToEdit) //Shared field population for both editing constructors
    {
        TaskSaveButton.Text = "Update";
        TaskTitleLabel.Text = "Edit a care moment";

        TaskNameEntry.Text = taskToEdit.TaskName;
        DescriptionEntry.Text = taskToEdit.TaskDescription;
        _priority = Math.Clamp(taskToEdit.TaskPriority, MinPriority, MaxPriority);
        RefreshPriorityState();
        InitializeBabyProfilePicker(taskToEdit.AssociatedBabyId);
    }

    //Shows the priority and dims the button that can't go further (1 and 10)
    private void RefreshPriorityState()
    {
        PriorityLabel.Text = _priority.ToString();
        PriorityDecreaseButton.IsEnabled = _priority > MinPriority;
        PriorityIncreaseButton.IsEnabled = _priority < MaxPriority;
    }

    private void OnPriorityDecreaseClicked(object? sender, EventArgs e)
    {
        if (_priority > MinPriority)
        {
            _priority--;
            RefreshPriorityState();
        }
    }

    private void OnPriorityIncreaseClicked(object? sender, EventArgs e)
    {
        if (_priority < MaxPriority)
        {
            _priority++;
            RefreshPriorityState();
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) //Exits page without saving anything
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        TaskSaveButton.IsEnabled = false; //Blocks a second tap from saving twice while the save is in flight

        try
        {
            bool isFormValid = await ValidateForm(); //Validates form before saving
            if (!isFormValid)
            {
                return;
            }

            if (!IsEditing) //New Task
            {
                await SaveNewTask();
            }

            else //edited task
            {
                Debug.WriteLine("Saving Edited Task");
                await SaveEditedTask();
            }
        }
        catch (DbCommunicationException) //Nothing was saved and the modal stays open, so the user can retry without retyping
        {
            string itemLabel = IsAppointmentCheckBox.IsChecked ? "appointment" : "task";
            await AlertService.ShowWriteFailedAsync(this, $"Couldn't save your {itemLabel}", "Everything you entered is still here. Please try again in a moment.");
        }
        finally
        {
            TaskSaveButton.IsEnabled = true;
        }
    }

    //Single place where form controls are turned into a model. Passing existingTask preserves identity and completion state while editing.
    private TaskModel BuildTaskFromForm(TaskModel? existingTask = null)
    {
        int priority = _priority;
        //An Entry that was never touched returns null, and the database columns are NOT NULL
        string taskName = TaskNameEntry.Text ?? string.Empty;
        string taskDescription = DescriptionEntry.Text ?? string.Empty;

        if (IsAppointmentCheckBox.IsChecked) //Appointments additionally carry the scheduling fields
        {
            return new AppointmentModel(LocationEntry.Text ?? string.Empty, DateEntry.Date, StartingTimeEntry.Time, EndingTimeEntry.Time, priority, taskName, taskDescription)
            {
                Id = existingTask?.Id ?? Guid.NewGuid(),
                IsCompleted = existingTask?.IsCompleted ?? false,
                AssociatedBabyId = _selectedAssociatedBabyId
            };
        }

        return new TaskModel(priority, taskName, taskDescription)
        {
            Id = existingTask?.Id ?? Guid.NewGuid(),
            IsCompleted = existingTask?.IsCompleted ?? false,
            AssociatedBabyId = _selectedAssociatedBabyId
        };
    }

    private async Task SaveNewTask() //Task instead of void to allow for more consistent async/await usage in the method
    {
        TaskModel newTask = BuildTaskFromForm();
        await _taskListService.AddAsync(newTask);

        await Navigation.PopModalAsync(); //Closes AddTaskPage
        ToastService.Show(newTask is AppointmentModel ? ToastKind.AppointmentScheduled : ToastKind.TaskAdded);
    }

    private async Task SaveEditedTask()
    {
        TaskModel? existingTask = (TaskModel?)appointmentOnEdit ?? taskOnEdit;

        if (existingTask is null)
        {
            return;
        }

        bool shouldBeAppointment = IsAppointmentCheckBox.IsChecked;
        bool isCurrentlyAppointment = appointmentOnEdit is not null;
        TaskModel updatedTask = BuildTaskFromForm(existingTask);

        await _taskListService.UpdateAsync(updatedTask); //Also covers task <-> appointment conversion, handled by the service as one update

        await Navigation.PopModalAsync();

        if (shouldBeAppointment != isCurrentlyAppointment) //Conversions keep their own toast
        {
            ToastService.Show(shouldBeAppointment ? ToastKind.AppointmentScheduled : ToastKind.TaskAdded);
            return;
        }

        ToastService.Show(ToastKind.TaskEdited);
    }

    private void InitializeBabyProfilePicker(Guid? selectedAssociatedBabyId = null)
    {
        (Guid? resolvedBabyId, string label) = _babyFilterService.ResolveSelection(selectedAssociatedBabyId, "Unassigned");

        _selectedAssociatedBabyId = resolvedBabyId;
        BabyProfileSelectionButton.Text = label;
    }

    private async void OnSelectBabyProfileClicked(object? sender, EventArgs e)
    {
        Dictionary<string, Guid?> selectionOptions = _babyFilterService.BuildOptions("Unassigned");

        string selectedOption = await DisplayActionSheetAsync("Select baby", "Cancel", null, selectionOptions.Keys.ToArray());

        if (string.IsNullOrEmpty(selectedOption) || selectedOption == "Cancel")
        {
            return;
        }

        _selectedAssociatedBabyId = selectionOptions[selectedOption];
        BabyProfileSelectionButton.Text = selectedOption;
    }

    private async Task<bool> ValidateForm()
    {
        //Some data validation making sure all required fields are filled out before saving
        if (string.IsNullOrWhiteSpace(TaskNameEntry.Text))
        {
            await DisplayAlertAsync("Required Field Missing", "Please fill in the task name.", "OK");
            return false;
        }

        if (IsAppointmentCheckBox.IsChecked && string.IsNullOrWhiteSpace(LocationEntry.Text))
        {
            await DisplayAlertAsync("Required Field Missing", "Please fill in the task location.", "OK");
            return false;
        }

        if ((StartingTimeEntry.Time >= EndingTimeEntry.Time) && IsAppointmentCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Invalid Time Range", "The starting time must be earlier than the ending time.", "OK");
            return false;
        }

        return true;
    }

    //Android only. The app default pans the whole window for the keyboard, which leaves Cancel and Save behind it. While the form is
    //showing, the page resizes above the keyboard instead (as on ChatPage), so the pinned Cancel/Save row stays in view (#176)
    protected override void OnAppearing()
    {
        base.OnAppearing();

        SetSoftInputMode(WindowSoftInputModeAdjust.Resize);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        SetSoftInputMode(WindowSoftInputModeAdjust.Pan);
    }

    private static void SetSoftInputMode(WindowSoftInputModeAdjust mode) =>
        Microsoft.Maui.Controls.Application.Current?.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(mode);
}
