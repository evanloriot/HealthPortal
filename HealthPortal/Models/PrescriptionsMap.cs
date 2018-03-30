using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("PrescriptionsMap")]
    public class PrescriptionsMap
    {
        [Key]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [Key]
        [ForeignKey("Prescription")]
        public int PrescriptionID { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Prescriptions Prescription { get; set; }
    }
}