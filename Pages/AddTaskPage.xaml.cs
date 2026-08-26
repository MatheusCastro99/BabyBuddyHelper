using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Diagnostics;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    private readonly ITaskListService _taskListService;
    TaskModel? taskOnEdit;
    AppointmentModel? appointmentOnEdit;
    bool isEditing = false; //false by default, meaning most of tasks are expected to be new tasks

    public AddTaskPage(ITaskListService taskListService, DateTime? dateTime = null) //Regular constructor called by New Task button
    {                                                                                //on ChecklistPage or clicking an empty time cell on
        InitializeComponent();                                                        //CalendarPage

        this._taskListService = taskListService;

        if (!(dateTime is null)) //Determines which Page modal is being called from.
        {                        //If it's from CalendarPage, fill information from time cell clicked
            IsAppointmentCheckBox.IsChecked = true;
            DateEntry.Date = dateTime;
            StartingTimeEntry.Time = dateTime.Value.TimeOfDay;
            EndingTimeEntry.Time = dateTime.Value.TimeOfDay.Add(new TimeSpan(01, 0, 0));
        }

        Debug.WriteLine("Creating New Task");
    }

    public AddTaskPage(ITaskListService taskListService, TaskModel taskOnEdit) //Constructor that will be triggered on
    {                                                                           //EditNoteIcon click for regular tasks
        InitializeComponent();

        this._taskListService = taskListService;
        this.taskOnEdit = taskOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked() behavior

        TaskNameEntry.Text = taskOnEdit.TaskName; //Populate fields with taskOnEdit info
        DescriptionEntry.Text = taskOnEdit.TaskDescription;
        PriorityStepper.Value = taskOnEdit.TaskPriority;

        Debug.WriteLine("Editing a regular task");
    }

    public AddTaskPage(ITaskListService taskListService, AppointmentModel appointmentOnEdit) //Constructor that is triggered on
    {                                                                                         //EditNoteIcon click for appointments
        InitializeComponent();

        this._taskListService = taskListService;
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

        Debug.WriteLine("Editing an appointment");
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

        if (IsAppointmentCheckBox.IsChecked) //Checks to see if new task being entered is an appointment
        {
            string appointmentLocation = LocationEntry.Text;
            DateTime? appointmentDate = DateEntry.Date; //Implemented DatePicker instead of regular text field
            TimeSpan? appointmentStartTime = StartingTimeEntry.Time; //Implemented TimePicker instead of regular text field
            TimeSpan? appointmentEndTime = EndingTimeEntry.Time;

            AppointmentModel newAppointment = new(appointmentLocation, appointmentDate, appointmentStartTime, appointmentEndTime, priority, taskName, taskDescription);
            _taskListService.Add(newAppointment);

            await Navigation.PopModalAsync(); //Closes AddTaskPage
        }

        else //thread of execution for non-appointment task
        {
            TaskModel newTask = new(priority, taskName, taskDescription);
            _taskListService.Add(newTask);

            await Navigation.PopModalAsync();
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
                Id = appointmentOnEdit.Id //Preserves TaskId for database update
            };

            _taskListService.Update(updatedAppt); //Updates appointment in task list by reference

            await Navigation.PopModalAsync();
        }

        else // Regular Task editing Case
        {
            if (IsAppointmentCheckBox.IsChecked) //Checks if user is trying to convert existing regular task into an appointment
            {
                _taskListService.Remove(taskOnEdit); //Removes task from list entirely and resaves it as an appointment
                await SaveNewTask();
                return;
            }

            taskOnEdit?.TaskName = TaskNameEntry.Text;
            taskOnEdit?.TaskDescription = DescriptionEntry.Text;
            taskOnEdit?.TaskPriority = Convert.ToInt32(PriorityStepper.Value);

            await Navigation.PopModalAsync();
        }
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