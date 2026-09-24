namespace BabyBuddyHelper.Models
{
    public class AppointmentModel : TaskModel
    {
        //Property definition
        public string AppointmentLocation { get; set; } = string.Empty;
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentStartTime { get; set; }
        public TimeSpan? AppointmentEndTime { get; set; }

        //Computed properties to get the full DateTime for SfScheduler
        public DateTime SchedulerStartTime => AppointmentDate!.Value.Date + AppointmentStartTime!.Value;
        public DateTime SchedulerEndTime => AppointmentDate!.Value.Date + AppointmentEndTime!.Value;



        //Parameterless constructor for database materialization (EF Core). App code uses the constructor below.
        private AppointmentModel()
            : base()
        {
        }

        //Constructor
        public AppointmentModel(string appointmentLocation, DateTime? appointmentDate, TimeSpan? appointmentStartTime, TimeSpan? appointmentEndTime, int priority, string name, string description)
            : base(priority, name, description)
        {
            AppointmentLocation = appointmentLocation;
            AppointmentDate = DateTime.SpecifyKind(appointmentDate!.Value, DateTimeKind.Local);
            AppointmentStartTime = appointmentStartTime;
            AppointmentEndTime = appointmentEndTime;
        }
    }
}
