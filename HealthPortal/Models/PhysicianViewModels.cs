using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class PhysicianIndexViewModel
    {
        public virtual IPagedList<ApplicationUser> Patients { get; set; }
    }

    public class ViewPatientViewModel
    {
        public virtual ApplicationUser Patient { get; set; }
        public virtual IList<Diagnosis> Diagnoses { get; set; }
        public virtual IList<Prescriptions> Prescriptions { get; set; }
        public virtual IList<MedicalHistory> MedicalHistory { get; set; }
    }

    public class DiagnosisGrouping
    {
        public string DiagnosisName { get; set; }
        public double Percent { get; set; }
    }

    public class ViewPatientDiagnosisBreakdownViewModel
    {
        public IList<DiagnosisGrouping> Rows { get; set; }
    }
}