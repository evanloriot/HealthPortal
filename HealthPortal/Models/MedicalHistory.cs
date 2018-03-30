using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("MedicalHistory")]
    public class MedicalHistory
    {
        [Key]
        public int MedicalHistoryID { get; set; }

        [ForeignKey("User")]
        public string UserID { get; set; }

        public string Details { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}