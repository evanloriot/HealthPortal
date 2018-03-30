using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("CheckUpResponse")]
    public class CheckUpResponse
    {
        [Key]
        public int ResponseID { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentID { get; set; }

        public string Q1A { get; set; }
        public string Q2A { get; set; }
        public string Q3A { get; set; }
        public string Q4A { get; set; }
        public string Q5A { get; set; }
        public string Q6A { get; set; }
        public string Q7A { get; set; }
        public string Q8A { get; set; }
        public string Q9A { get; set; }
        public string Q10A { get; set; }

        public virtual Appointments Appointment { get; set; }
    }
}