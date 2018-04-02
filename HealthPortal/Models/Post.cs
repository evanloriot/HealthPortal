using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    [Table("Post")]
    public class Post
    {
        [Key]
        public int PostID { get; set; }

        [ForeignKey("User")]
        public string UserID { get; set; }

        public DateTime TimeDate { get; set; }

        [ForeignKey("Thread")]
        public int ThreadID { get; set; }

        public string Message { get; set; }

        public bool Deleted { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Thread Thread { get; set; }
    }
}