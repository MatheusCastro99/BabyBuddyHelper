using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Services;
using System.Diagnostics;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    private readonly ITaskListService _taskListService;
    private readonly IBabyFilterService _babyFilterService;
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
        PriorityStepper.Value = taskToEdit.TaskPriority;
        InitializeBabyProfilePicker(taskToEdit.AssociatedBabyId);
    }

    private void OnPriorityDecreaseClicked(object? sender, EventArgs e)
    {
        if (PriorityStepper.Value > PriorityStepper.Minimum)
        {
            PriorityStepper.Value -= 1;
        }
    }

    private void OnPriorityIncreaseClicked(object? sender, EventArgs e)
    {
        if (PriorityStepper.Value < PriorityStepper.Maximum)
        {
            PriorityStepper.Value += 1;
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
        finally
        {
            TaskSaveButton.IsEnabled = true;
        }
    }

    //Single place where form controls are turned into a model. Passing existingTask preserves identity and completion state while editing.
    private TaskModel BuildTaskFromForm(TaskModel? existingTask = null)
    {
        int priority = Convert.ToInt32(PriorityStepper.Value);
        string taskName = TaskNameEntry.Text;
        string taskDescription = DescriptionEntry.Text;

        if (IsAppointmentCheckBox.IsChecked) //Appointments additionally carry the scheduling fields
        {
            return new AppointmentModel(LocationEntry.Text, DateEntry.Date, StartingTimeEntry.Time, EndingTimeEntry.Time, priority, taskName, taskDescription)
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
}
