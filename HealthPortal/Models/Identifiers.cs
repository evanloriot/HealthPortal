using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthPortal.Models
{
    public class Identifiers
    {
        [Key, ForeignKey("User")]
        public string IdentifierID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }
        
        public string EmergencyPhone { get; set; }

        [Required]
        public string SSN { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}