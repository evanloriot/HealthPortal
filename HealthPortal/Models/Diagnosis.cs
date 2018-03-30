using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Diagnosis")]
    public class Diagnosis
    {
        [Key]
        public int DiagnosisID { get; set; }

        public string DiagnosisName { get; set; }
    }
}