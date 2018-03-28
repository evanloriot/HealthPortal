using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Appointments")]
    public class Appointments
    {
        [Key]
        public int AppointmentID { get; set; }

        [ForeignKey("Patient")]
        public string PatientID { get; set; }

        [ForeignKey("Physician")]
        public string PhysicianID { get; set; }

        public DateTime TimeDate { get; set; }

        public string Reason { get; set; }


        public virtual ApplicationUser Patient { get; set; }

        public virtual ApplicationUser Physician { get; set; }
    }
}