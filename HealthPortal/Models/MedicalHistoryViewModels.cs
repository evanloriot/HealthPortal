using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class MedicalHistoryIndexViewModel
    {
        public virtual IList<MedicalHistory> History { get; set; }
        public IPagedList<ApplicationUser> Patients { get; set; }
        public IDictionary<string, List<MedicalHistory>> PatientHistories { get; set; }
    }

    public class AddMedicalHistoryViewModel
    {
        public string ID { get; set; }
        public string Details { get; set; }
    }
}