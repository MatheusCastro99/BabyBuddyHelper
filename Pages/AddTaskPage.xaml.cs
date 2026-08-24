using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Diagnostics;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    //List<TaskModel> currentTasks = new();
    private readonly ITaskListService _taskListService;
    TaskModel? taskOnEdit;
    AppointmentModel? appointmentOnEdit;
    bool isEditing = false; //false by default, meaning most of tasks are expected to be new tasks

	public AddTaskPage(ITaskListService taskListService) //Regular constructor called by New Task button
	{
		InitializeComponent();

        this._taskListService = taskListService;

        Debug.WriteLine("Creating New Task");
    }

    public AddTaskPage(ITaskListService taskListService, TaskModel taskOnEdit) //Constructor that will be triggered on
    {                                                                                                  //EditNoteIcon click for regular tasks
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
    {                                                                                                                //EditNoteIcon click for appointments
        InitializeComponent();

        this._taskListService = taskListService;
        this.appointmentOnEdit = appointmentOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked() behavior

        TaskNameEntry.Text = appointmentOnEdit.TaskName; //Populate fields with appointmentOnEdit info
        DescriptionEntry.Text = appointmentOnEdit.TaskDescription;
        PriorityStepper.Value = appointmentOnEdit.TaskPriority;
        IsAppointmentCheckBox.IsChecked = true;
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

        //isEditing is false by default, and modified by the constructors when clicking on EditNoteIcon
        if (!isEditing) //New Task
        {
            SaveNewTask();
        }

        else //edited task
        {
            SaveEditedTask();
        }
    }

    private async void SaveNewTask()
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

    private async void SaveEditedTask()
    {
        if (appointmentOnEdit != null) //appointment instance editing case
        {

            if (!IsAppointmentCheckBox.IsChecked) //Checks if user it trying to convert existing appointment to regular task
            {
                _taskListService.Remove(appointmentOnEdit); //Removes Appointment Instance of task list (prevents duplicates)
                SaveNewTask(); //Resaves task from 0 as a regular non-appointment task
            }

            TimeSpan? appointmentStartTime = StartingTimeEntry.Time; //If there is no task type conversion, consolidate variables (passed through argument reference)
            TimeSpan? appointmentEndTime = EndingTimeEntry.Time;
            appointmentOnEdit?.TaskName = TaskNameEntry.Text;
            appointmentOnEdit?.TaskDescription = DescriptionEntry.Text;
            appointmentOnEdit?.TaskPriority = Convert.ToInt32(PriorityStepper.Value);
            appointmentOnEdit?.AppointmentLocation = LocationEntry.Text;
            appointmentOnEdit?.AppointmentStartTime = appointmentStartTime;
            appointmentOnEdit?.AppointmentEndTime = appointmentEndTime;

            await Navigation.PopModalAsync();
        }

        else // Regular Task editing Case
        {
            if(IsAppointmentCheckBox.IsChecked) //Checks if user is trying to convert existing regular task into an appointment
            {
                _taskListService.Remove(taskOnEdit); //Removes task from list entirely and resaves it as an appointment
                SaveNewTask();
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

        if (IsAppointmentCheckBox.IsChecked && string.IsNullOrWhiteSpace(LocationEntry.Text)) //MOVE VALIDATION TO A SEPARATE METHOD
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