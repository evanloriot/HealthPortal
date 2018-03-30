using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Prescriptions")]
    public class Prescriptions
    {
        [Key]
        public int PrescriptionID { get; set; }

        public string PrescriptionName { get; set; }

        [ForeignKey("Type")]
        public int PrescriptionTypeID { get; set; }

        public virtual PrescriptionType Type { get; set; }
    }
}