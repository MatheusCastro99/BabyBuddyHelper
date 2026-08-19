using System;
using System.Collections.Generic;
using System.Text;

namespace BabyBuddyHelper.Models
{
    public class AppointmentModel : TaskModel
    {
        //Property definition
        public string AppointmentLocation { get; set; }
        public DateTime? AppointmentTime { get; set; }

        //Constructor
        public AppointmentModel(string appointmentLocation, DateTime? appointmentTime, int priority, string name, string description) 
            : base(priority, name, description)
        {
            AppointmentLocation = appointmentLocation;
            AppointmentTime = appointmentTime;
        }
    }
}
