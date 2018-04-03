using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("DiagnosisMap")]
    public class DiagnosisMap
    {
        [Key]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [Key]
        [ForeignKey("Diagnosis")]
        public int DiagnosisID { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Diagnosis Diagnosis { get; set; }
    }
}