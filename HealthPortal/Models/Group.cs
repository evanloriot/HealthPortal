using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        public int GroupID { get; set; }

        public string GroupName { get; set; }
    }
}