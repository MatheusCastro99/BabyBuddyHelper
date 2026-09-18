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
    private readonly IBabyProfileService _babyProfileService;
    private readonly IBabyFilterService _babyFilterService;
    private Guid? _selectedBabyFilterId;
    public ObservableCollection<AppointmentModel>? TaskList { get; private set; }//Will hold instances of AppointmentModel

    public CalendarPage(ITaskListService taskListService, IBabyProfileService babyProfileService, IBabyFilterService babyFilterService)
    {
        InitializeComponent();

        _taskListService = taskListService;
        _babyProfileService = babyProfileService;
        _babyFilterService = babyFilterService;
        _taskListService.Tasks.CollectionChanged += (s, e) => //Subscribe to the CollectionChanged event of the TaskListService's Tasks collection
        {
            CalendarRefresh();
        };
        _babyProfileService.BabyProfiles.CollectionChanged += (s, e) =>
        {
            CalendarRefresh();
        };

        CalendarRefresh();

        BindingContext = this;
    }

    private void CalendarRefresh()
    {
        RefreshSelectedBabyFilterLabel();

        TaskList = _taskListService.GetAppointments(_selectedBabyFilterId).ToObservableCollection(); //Update TaskList with the latest appointments

        var schedulerAppointments = new ObservableCollection<SchedulerAppointment>(
            _taskListService.GetAppointments(_selectedBabyFilterId)   //Filter instances of tasks in _taskListService that are AppointmentModels
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

    private async void OnCalendarBabyFilterClicked(object sender, EventArgs e)
    {
        Dictionary<string, Guid?> filterOptions = _babyFilterService.BuildOptions("All babies");

        string selectedOption = await DisplayActionSheetAsync("Filter calendar by baby", "Cancel", null, filterOptions.Keys.ToArray());

        if (string.IsNullOrEmpty(selectedOption) || selectedOption == "Cancel")
        {
            return;
        }

        _selectedBabyFilterId = filterOptions[selectedOption];
        CalendarBabyFilterButton.Text = selectedOption;
        CalendarRefresh();
    }

    private void RefreshSelectedBabyFilterLabel()
    {
        (Guid? resolvedBabyId, string label) = _babyFilterService.ResolveSelection(_selectedBabyFilterId, "All babies");

        _selectedBabyFilterId = resolvedBabyId;
        CalendarBabyFilterButton.Text = label;
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
            await AddNewAppointment(e.Date);
        }
    }

    private async Task AddNewAppointment(DateTime? AppointmentDate) //Triggers AddTaskPage Modal with the specified DateTime from event handler
    {
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, _babyFilterService, AppointmentDate));
    }

    private async Task EditAppointment(AppointmentModel appointmentToEdit) //Triggers AddTaskPage Modal with the specified AppointmentModel
    {                                                                       //from event handler
        if (appointmentToEdit is null) return;
        await Navigation.PushModalAsync(new AddTaskPage(_taskListService, _babyFilterService, appointmentToEdit));
    }
}