﻿using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class PrescriptionsIndexViewModel
    {
        public virtual IList<Prescriptions> Prescriptions { get; set; }
        public virtual IList<ApplicationUser> PatientList { get; set; }
    }

    public class ViewPatientPrescriptionViewModel
    {
        public string PatientName { get; set; }
        public virtual IList<Prescriptions> Prescriptions { get; set; }
        public string PatientID { get; set; }
    }

    public class AddPrescriptionViewModel
    {
        [Required]
        [Display(Name = "Prescription Name")]
        public string PrescriptionName { get; set; }
        public virtual IList<PrescriptionType> Types { get; set; }

        [Required]
        public int PrescriptionTypeID { get; set; }
    }

    public class AddPrescriptionTypeViewModel
    {
        [Required]
        [Display(Name = "Prescription Type")]
        public string Type { get; set; }
    }

    public class ManagePrescriptionTypesViewModel
    {
        public virtual IList<PrescriptionType> Types { get; set; }
    }

    public class AddPrescriptionToPatientViewModel
    {
        public virtual IList<Prescriptions> Prescriptions { get; set; }
        public int PrescriptionID { get; set; }
        public string PatientID { get; set; }
        public string PatientName { get; set; }
    }

    public class ViewPrescriptionsViewModel
    {
        public virtual IList<Prescriptions> Prescriptions { get; set; }
    }
}