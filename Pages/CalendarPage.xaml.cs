using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using BabyBuddyHelper.Services;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace BabyBuddyHelper.Pages;

public partial class CalendarPage : ContentPage
{
	public DateTime? SelectedDate { get; set; } = DateTime.Today;
    private readonly ITaskListService _taskListService; //Dependency Injection for TaskListService
    public ObservableCollection<AppointmentModel> TaskList { get; private set; }//Will hold instances of AppointmentModel

    public CalendarPage(ITaskListService taskListService)
	{
		InitializeComponent();

        _taskListService = taskListService;
        _taskListService.Tasks.CollectionChanged += (s, e) => //Subscribe to the CollectionChanged event of the TaskListService's Tasks collection
        {
            CalendarRefresh();
        };

        CalendarRefresh();

        BindingContext = this;
    }

    private void CalendarRefresh()
    {
        TaskList = _taskListService.GetAppointments().ToObservableCollection(); //Update TaskList with the latest appointments

        var schedulerAppointments = new ObservableCollection<SchedulerAppointment>(
            _taskListService.GetAppointments()                        //Filter instances of tasks in _taskListService that are AppointmentModels
            .Select(appt => new SchedulerAppointment                    //Then, for each appt filtered, creates a SchedulerAppointment counterpart
            {
                Subject = appt.TaskName,
                StartTime = appt.SchedulerStartTime,
                EndTime = appt.SchedulerEndTime,
                Location = appt.AppointmentLocation
            }));

        Calendar.AppointmentsSource = schedulerAppointments; //Actual Binding for sfScheduler
    }

    //Implement add and edit functionaly for appointments through tapped event, triggering modal AddTaskPage
    //with prefilled data for editing, or empty data (exept by date) for adding a new appointment.
}