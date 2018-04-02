using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HealthPortal.Models
{
    public class ManageGroupsViewModel
    {
        public IPagedList<Group> Groups { get; set; }
    }

    public class AddGroupViewModel
    {
        [Required]
        [Display(Name = "Group Name")]
        [MaxLength(30)]
        public string GroupName { get; set; }
    }

    public class ForumIndexViewModel
    {
        public IPagedList<Group> Groups { get; set; }
    }

    public class ViewGroupViewModel
    {
        public Group Group { get; set; }
        public IPagedList<Thread> Threads { get; set; }
    }

    public class AddThreadViewModel
    {
        public int GroupID { get; set; }
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime TimeDate { get; set; }
    }

    public class ViewThreadViewModel
    {
        public Thread Thread { get; set; }
        public IPagedList<Post> Posts { get; set; }
    }

    public class AddPostViewModel
    {
        public Thread Thread { get; set; }

        public string UserID { get; set; }
        public DateTime TimeDate { get; set; }
        public int ThreadID { get; set; }

        [Required]
        public string Message { get; set; }
    }
}