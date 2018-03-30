using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class AddResponseViewModel
    {
        public Appointments Appointment { get; set; }

        public string PhysicianName { get; set; }

        public int AppointmentID { get; set; }

        [Required]
        [Display(Name = "Question 1: Do you have any current medical complaints?")]
        public string Q1A { get; set; }

        [Required]
        [Display(Name = "Question 2: If you answered yes to question 4, please select the reason of highest priority.")]
        public string Q2A { get; set; }

        [Required]
        [Display(Name = "Question 3: Do you consume tobacco or other carcinogens?")]
        public string Q3A { get; set; }
        [Required]
        [Display(Name = "Question 4: Have you been getting an adequate amount of physical activity in the time since your last checkup?")]
        public string Q4A { get; set; }

        [Required]
        [Display(Name = "Question 5: Do you drink alcohol on an exceptional basis?")]
        public string Q5A { get; set; }

        [Required]
        [Display(Name = "Question 6: Has your appetite been stable since your last checkup?")]
        public string Q6A { get; set; }

        [Required]
        [Display(Name = "Question 7: Please rate your energy level: 1 low, 9 high.")]
        public string Q7A { get; set; }

        [Required]
        [Display(Name = "Question 8: Have you been getting enough sleep as of late?")]
        public string Q8A { get; set; }

        [Required]
        [Display(Name = "Question 9: Are you currently taking any medications?")]
        public string Q9A { get; set; }

        [Required]
        [Display(Name = "Question 10: Have you experienced any debilitating symptons such as dizziness, fatigue, or vision problems?")]
        public string Q10A { get; set; }
    }
}