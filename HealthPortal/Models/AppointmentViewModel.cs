using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class AppointmentIndexViewModel
    {
        public virtual Appointments PatientAppointment { get; set; }
        public virtual Appointments Appointment { get; set; }
    }

    public class ScheduleAppointmentViewModel
    {
        public virtual IList<Appointments> PhysicianAppointments { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, bool> TimeDates { get; set; }
    }

    public class ViewAppointmentsViewModel
    {
        public virtual IPagedList<Appointments> Appointments { get; set; }
    }
}