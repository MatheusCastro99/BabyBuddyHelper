using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;


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
                Id = appt.Id,
                Subject = appt.TaskName,
                StartTime = appt.SchedulerStartTime,
                EndTime = appt.SchedulerEndTime,
                Location = appt.AppointmentLocation
            }));

        Calendar.AppointmentsSource = schedulerAppointments; //Actual Binding for sfScheduler
    }

    private async void OnCalendarDoubleTapped(object? sender, SchedulerDoubleTappedEventArgs e)
    {
        if (e.Element.ToString().Equals("Appointment")) //Editing an existing appointment through Calendar
        {
            var schedulerAppointment = e.Appointments.FirstOrDefault() as SchedulerAppointment;
            if (schedulerAppointment is null) return;

            var appointmentToEdit = _taskListService.GetAppointments()
                .FirstOrDefault(appt => appt.Id.Equals(schedulerAppointment.Id)); //Retrieve first appointment from TaskListService that
                                                                                  //matches the Id of the tapped SchedulerAppointment
            if (appointmentToEdit != null)
            {
                await EditAppointment(appointmentToEdit);
            }
        }
        else                                            //Creating a new appointment through Calendar
        {
            AddNewAppointment(e.Date);
        }
    }

    private async Task AddNewAppointment(DateTime? AppointmentDate) //Triggers AddTaskPage Modal with the specified DateTime from event handler
    {
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, AppointmentDate));
    }

    private async Task EditAppointment(AppointmentModel appointmentToEdit) //Triggers AddTaskPage Modal with the specified AppointmentModel
    {                                                                       //from event handler
        if (appointmentToEdit is null) return;
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, appointmentToEdit));
    }
}