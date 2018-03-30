using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("PrescriptionType")]
    public class PrescriptionType
    {
        [Key]
        public int PrescriptionTypeID { get; set; }

        public string Type { get; set; }
    }
}