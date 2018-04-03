using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class DiagnosisIndexViewModel
    {
        public virtual IList<Diagnosis> Diagnoses { get; set; }
    }
}