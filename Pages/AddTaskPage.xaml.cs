using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Services;
using System.Diagnostics;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    private readonly ITaskListService _taskListService;
    private readonly IBabyProfileService _babyProfileService;
    private Guid? _selectedAssociatedBabyId;
    TaskModel? taskOnEdit;
    AppointmentModel? appointmentOnEdit;
    bool isEditing = false; //false by default, meaning most of tasks are expected to be new tasks

    public AddTaskPage(ITaskListService taskListService, IBabyProfileService babyProfileService, DateTime? dateTime = null) //Regular constructor called by New Task button
    {                                                                                                     //on ChecklistPage or clicking an empty time cell on
        InitializeComponent();                                                        //CalendarPage

        this._taskListService = taskListService;
        this._babyProfileService = babyProfileService;
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

    public AddTaskPage(ITaskListService taskListService, IBabyProfileService babyProfileService, TaskModel taskOnEdit) //Constructor that will be triggered on
    {                                                                                                    //EditNoteIcon click for regular tasks
        InitializeComponent();
        TaskSaveButton.Text = "Update";
        TaskTitleLabel.Text = "Edit a care moment";

        this._taskListService = taskListService;
        this._babyProfileService = babyProfileService;
        this.taskOnEdit = taskOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked() behavior

        TaskNameEntry.Text = taskOnEdit.TaskName; //Populate fields with taskOnEdit info
        DescriptionEntry.Text = taskOnEdit.TaskDescription;
        PriorityStepper.Value = taskOnEdit.TaskPriority;
        InitializeBabyProfilePicker(taskOnEdit.AssociatedBabyId);

        Debug.WriteLine("Editing a regular task");
    }

    public AddTaskPage(ITaskListService taskListService, IBabyProfileService babyProfileService, AppointmentModel appointmentOnEdit) //Constructor that is triggered on
    {                                                                                                                  //EditNoteIcon click for appointments
        InitializeComponent();
        TaskSaveButton.Text = "Update";
        TaskTitleLabel.Text = "Edit a care moment";

        this._taskListService = taskListService;
        this._babyProfileService = babyProfileService;
        this.appointmentOnEdit = appointmentOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked() behavior

        TaskNameEntry.Text = appointmentOnEdit.TaskName; //Populate fields with appointmentOnEdit info
        DescriptionEntry.Text = appointmentOnEdit.TaskDescription;
        PriorityStepper.Value = appointmentOnEdit.TaskPriority;
        IsAppointmentCheckBox.IsChecked = true;
        DateEntry.Date = appointmentOnEdit.AppointmentDate;
        StartingTimeEntry.Time = appointmentOnEdit.AppointmentStartTime;
        EndingTimeEntry.Time = appointmentOnEdit.AppointmentEndTime;
        LocationEntry.Text = appointmentOnEdit.AppointmentLocation;
        InitializeBabyProfilePicker(appointmentOnEdit.AssociatedBabyId);

        Debug.WriteLine("Editing an appointment");
    }

    private void OnPriorityDecreaseClicked(object sender, EventArgs e)
    {
        if (PriorityStepper.Value > PriorityStepper.Minimum)
        {
            PriorityStepper.Value -= 1;
        }
    }

    private void OnPriorityIncreaseClicked(object sender, EventArgs e)
    {
        if (PriorityStepper.Value < PriorityStepper.Maximum)
        {
            PriorityStepper.Value += 1;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e) //Exits page without saving anything
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        bool isFormValid = await ValidateForm(); //Validates form before saving
        if (!isFormValid)
        {
            return;
        }

        //isEditing is false by default, and modified by the constructors when clicking on EditNoteIcon
        if (!isEditing) //New Task
        {
            await SaveNewTask();
        }

        else //edited task
        {
            Debug.WriteLine("Saving Edited Task");
            await SaveEditedTask();
        }
    }

    private async Task SaveNewTask() //Task instead of void to allow for more consistent async/await usage in the method
    {
        int priority = Convert.ToInt32(PriorityStepper.Value); //Consolidate entries into variables
        string taskName = TaskNameEntry.Text;
        string taskDescription = DescriptionEntry.Text;
        Guid? selectedAssociatedBabyId = GetSelectedAssociatedBabyId();

        if (IsAppointmentCheckBox.IsChecked) //Checks to see if new task being entered is an appointment
        {
            string appointmentLocation = LocationEntry.Text;
            DateTime? appointmentDate = DateEntry.Date; //Implemented DatePicker instead of regular text field
            TimeSpan? appointmentStartTime = StartingTimeEntry.Time; //Implemented TimePicker instead of regular text field
            TimeSpan? appointmentEndTime = EndingTimeEntry.Time;

            AppointmentModel newAppointment = new(appointmentLocation, appointmentDate, appointmentStartTime, appointmentEndTime, priority, taskName, taskDescription)
            {
                AssociatedBabyId = selectedAssociatedBabyId,
                AssociatedBabyName = ResolveAssociatedBabyName(selectedAssociatedBabyId)
            };
            _taskListService.Add(newAppointment);

            await Navigation.PopModalAsync(); //Closes AddTaskPage
            ToastService.Show(ToastKind.AppointmentScheduled);
        }

        else //thread of execution for non-appointment task
        {
            TaskModel newTask = new(priority, taskName, taskDescription)
            {
                AssociatedBabyId = selectedAssociatedBabyId,
                AssociatedBabyName = ResolveAssociatedBabyName(selectedAssociatedBabyId)
            };
            _taskListService.Add(newTask);

            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.TaskAdded);
        }
    }

    private async Task SaveEditedTask()
    {
        if (appointmentOnEdit != null) //appointment instance editing case
        {
            if (!IsAppointmentCheckBox.IsChecked) //Checks if user it trying to convert existing appointment to regular task
            {
                _taskListService.Remove(appointmentOnEdit); //Removes Appointment Instance of task list (prevents duplicates)
                await SaveNewTask();                         //Resaves task from 0 as a regular non-appointment task
                return;
            }

            AppointmentModel updatedAppt = new
            (
                LocationEntry.Text,
                DateEntry.Date,
                StartingTimeEntry.Time,
                EndingTimeEntry.Time,
                Convert.ToInt32(PriorityStepper.Value),
                TaskNameEntry.Text,
                DescriptionEntry.Text
            )
            {
                Id = appointmentOnEdit.Id, //Preserves TaskId for database update
                AssociatedBabyId = GetSelectedAssociatedBabyId(),
                AssociatedBabyName = ResolveAssociatedBabyName(GetSelectedAssociatedBabyId())
            };

            _taskListService.Update(updatedAppt); //Updates appointment in task list by reference

            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.TaskEdited);
        }

        else // Regular Task editing Case
        {
            if (IsAppointmentCheckBox.IsChecked) //Checks if user is trying to convert existing regular task into an appointment
            {
                _taskListService.Remove(taskOnEdit!); //Removes task from list entirely and resaves it as an appointment
                await SaveNewTask();
                return;
            }

            if (taskOnEdit is null)
            {
                return;
            }

            Guid? selectedAssociatedBabyId = GetSelectedAssociatedBabyId();

            TaskModel updatedTask = new
            (
                Convert.ToInt32(PriorityStepper.Value),
                TaskNameEntry.Text,
                DescriptionEntry.Text
            )
            {
                Id = taskOnEdit.Id,
                IsCompleted = taskOnEdit.IsCompleted,
                AssociatedBabyId = selectedAssociatedBabyId,
                AssociatedBabyName = ResolveAssociatedBabyName(selectedAssociatedBabyId)
            };

            _taskListService.Update(updatedTask);

            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.TaskEdited);
        }
    }

    private void InitializeBabyProfilePicker(Guid? selectedAssociatedBabyId = null)
    {
        if (!selectedAssociatedBabyId.HasValue || !_babyProfileService.BabyProfiles.Any(profile => profile.Id == selectedAssociatedBabyId.Value))
        {
            _selectedAssociatedBabyId = null;
            BabyProfileSelectionButton.Text = "Unassigned";
            return;
        }

        _selectedAssociatedBabyId = selectedAssociatedBabyId;
        BabyProfileSelectionButton.Text = _babyProfileService.BabyProfiles
            .First(profile => profile.Id == selectedAssociatedBabyId.Value)
            .Name;
    }

    private async void OnSelectBabyProfileClicked(object sender, EventArgs e)
    {
        Dictionary<string, Guid?> selectionOptions = new()
        {
            ["Unassigned"] = null
        };

        foreach (BabyModel profile in _babyProfileService.BabyProfiles)
        {
            string optionLabel = profile.Name;
            int duplicateCounter = 2;

            while (selectionOptions.ContainsKey(optionLabel))
            {
                optionLabel = $"{profile.Name} ({duplicateCounter})";
                duplicateCounter++;
            }

            selectionOptions[optionLabel] = profile.Id;
        }

        string selectedOption = await DisplayActionSheetAsync("Select baby", "Cancel", null, selectionOptions.Keys.ToArray());

        if (string.IsNullOrEmpty(selectedOption) || selectedOption == "Cancel")
        {
            return;
        }

        _selectedAssociatedBabyId = selectionOptions[selectedOption];
        BabyProfileSelectionButton.Text = selectedOption;
    }

    private Guid? GetSelectedAssociatedBabyId()
    {
        return _selectedAssociatedBabyId;
    }

    private string ResolveAssociatedBabyName(Guid? selectedAssociatedBabyId)
    {
        if (!selectedAssociatedBabyId.HasValue)
        {
            return "General";
        }

        BabyModel? matchedProfile = _babyProfileService.BabyProfiles
            .FirstOrDefault(profile => profile.Id == selectedAssociatedBabyId.Value);

        return string.IsNullOrWhiteSpace(matchedProfile?.Name) ? "General" : matchedProfile.Name;
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