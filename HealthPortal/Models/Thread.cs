using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Thread")]
    public class Thread
    {
        [Key]
        public int ThreadID { get; set; }

        public string Title { get; set; }

        [ForeignKey("Group")]
        public int GroupID { get; set; }

        public string Message { get; set; }

        public DateTime TimeDate { get; set; }

        public virtual Group Group { get; set; }
    }
}