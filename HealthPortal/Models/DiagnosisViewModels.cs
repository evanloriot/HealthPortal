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
        public virtual IList<ApplicationUser> PatientList { get; set; }
    }

    public class ViewPatientDiagnosisViewModel
    {
        public string PatientName { get; set; }
        public virtual IList<Diagnosis> Diagnoses { get; set; }
        public string PatientID { get; set; } 
    }

    public class AddDiagnosisViewModel
    {
        [Required]
        public string DiagnosisName { get; set; }
    }

    public class AddDiagnosisToPatientViewModel
    {
        public virtual IList<Diagnosis> Diagnoses { get; set; }
        public int DiagnosisID { get; set; }
        public string PatientID { get; set; }
        public string PatientName { get; set; }
    }

    public class ViewDiagnosesViewModel
    {
        public virtual IList<Diagnosis> Diagnoses { get; set; }
    }
}