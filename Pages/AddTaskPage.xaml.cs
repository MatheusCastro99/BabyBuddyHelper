using BabyBuddyHelper.Models;
using System.Diagnostics;

namespace BabyBuddyHelper.Pages;

public partial class AddTaskPage : ContentPage
{
    List<TaskModel> currentTasks = new();
    TaskModel? taskOnEdit;
    AppointmentModel? appointmentOnEdit;
    Action? onTaskSaved;
    bool isEditing = false; //false by default, meaning most of tasks are expected to be new tasks

	public AddTaskPage(List<TaskModel> currentTasks, Action? onTaskSaved = null) //Regular constructor called by New Task button
	{
		InitializeComponent();

        this.currentTasks = currentTasks;
        this.onTaskSaved = onTaskSaved;
	}

    public AddTaskPage(List<TaskModel> currentTasks, TaskModel taskOnEdit, Action? onTaskSaved = null) //Constructor that will be triggered on
    {                                                                                                  //EditNoteIcon click for regular task
        InitializeComponent();

        this.currentTasks = currentTasks; //Loads relevant data for editing operations
        this.onTaskSaved = onTaskSaved;
        this.taskOnEdit = taskOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked behavior

        TaskNameEntry.Text = taskOnEdit.TaskName; //Populate fields with taskOnEdit info
        DescriptionEntry.Text = taskOnEdit.TaskDescription;
        PriorityStepper.Value = taskOnEdit.TaskPriority;
    }

    public AddTaskPage(List<TaskModel> currentTasks, AppointmentModel appointmentOnEdit, Action? onTaskSaved = null) //Constructor for editing Appointments
    {
        InitializeComponent();

        this.currentTasks = currentTasks; //Loads relevant data for editing operations
        this.onTaskSaved = onTaskSaved;
        this.appointmentOnEdit = appointmentOnEdit;
        isEditing = true; //Sets isEditing to change OnSaveClicked behavior

        TaskNameEntry.Text = appointmentOnEdit.TaskName; //Populate fields with appointmentOnEdit info
        DescriptionEntry.Text = appointmentOnEdit.TaskDescription;
        PriorityStepper.Value = appointmentOnEdit.TaskPriority;
        IsAppointmentCheckBox.IsChecked = true;
        DateEntry.Text = appointmentOnEdit.AppointmentTime.ToString("d");
        LocationEntry.Text = appointmentOnEdit.AppointmentLocation;

        Debug.WriteLine("Constructor Reached");
    }

    private async void OnCancelClicked(object sender, EventArgs e) //Exits page without saving anything
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if(!isEditing) //new task
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
        int priority = Convert.ToInt32(PriorityStepper.Value);
        string taskName = TaskNameEntry.Text;
        string taskDescription = DescriptionEntry.Text;

        if (IsAppointmentCheckBox.IsChecked) //Checks to see if new task being entered is an appointment or not
        {
            string appointmentLocation = LocationEntry.Text;
            DateTime appointmentTime;
            DateTime.TryParse(DateEntry.Text, out appointmentTime); //Come up with a way to parse string into datetime

            AppointmentModel newAppointment = new(appointmentLocation, appointmentTime, priority, taskName, taskDescription);
            currentTasks.Add(newAppointment);

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }

        else
        {
            TaskModel newTask = new(priority, taskName, taskDescription);
            currentTasks.Add(newTask);

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }

    }

    private async void SaveEditedTask()
    {
        if (appointmentOnEdit != null)
        {
            DateTime appointmentTime;
            DateTime.TryParse(DateEntry.Text, out appointmentTime);

            appointmentOnEdit?.TaskName = TaskNameEntry.Text;
            appointmentOnEdit?.TaskDescription = DescriptionEntry.Text;
            appointmentOnEdit?.TaskPriority = Convert.ToInt32(PriorityStepper.Value);
            appointmentOnEdit?.AppointmentLocation = LocationEntry.Text;
            appointmentOnEdit?.AppointmentTime = appointmentTime;

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }

        else
        {
            taskOnEdit?.TaskName = TaskNameEntry.Text;
            taskOnEdit?.TaskDescription = DescriptionEntry.Text;
            taskOnEdit?.TaskPriority = Convert.ToInt32(PriorityStepper.Value);

            onTaskSaved.Invoke();
            await Navigation.PopModalAsync();
        }

    }
}