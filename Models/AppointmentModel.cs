using System;
using System.Collections.Generic;
using System.Text;

namespace BabyBuddyHelper.Models
{
    public class AppointmentModel : TaskModel
    {
        //Property definition
        public string AppointmentLocation { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentStartTime { get; set; }
        public TimeSpan? AppointmentEndTime { get; set; }



        //Constructor
        public AppointmentModel(string appointmentLocation, DateTime? appointmentDate, TimeSpan? appointmentStartTime, TimeSpan? appointmentEndTime, int priority, string name, string description) 
            : base(priority, name, description)
        {
            AppointmentLocation = appointmentLocation;
            AppointmentDate = appointmentDate;
            AppointmentStartTime = appointmentStartTime;
            AppointmentEndTime = appointmentEndTime;
        }
    }
}
