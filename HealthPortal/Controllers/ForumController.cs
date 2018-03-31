using HealthPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HealthPortal.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private const int DefaultPageSize = 10;

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ForumController()
        {
        }

        public ForumController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //GET: Forum/ManageGroups
        public ActionResult ManageGroups(int? page, ForumMessageId? message)
        {
            ViewBag.StatusText = message == ForumMessageId.AddGroupSuccess ? "Group added successfully."
                : message == ForumMessageId.DeleteGroupSuccess ? "Group successfully deleted."
                : "";

            bool result = User.IsInRole("Doctor");
            if (!result)
            {
                ViewBag.Error = "Please sign into a doctoral account to manage groups";
                return View("Error");
            }

            var db = new ApplicationDbContext();
            var groups = db.Groups.OrderBy(u => u.GroupName).ToList();

            var currentPage = page ?? 1;
            var model = new ManageGroupsViewModel
            {
                Groups = groups.ToPagedList(currentPage, DefaultPageSize)
            };

            return View(model);
        }

        //GET: Forum/AddGroup
        public ActionResult AddGroup()
        {
            var model = new AddGroupViewModel();
            return View(model);
        }

        //POST: Forum/AddGroup
        [HttpPost]
        public ActionResult AddGroup(AddGroupViewModel model)
        {
            var group = new Group
            {
                GroupName = model.GroupName
            };
            var db = new ApplicationDbContext();
            db.Groups.Add(group);
            db.SaveChanges();
            return RedirectToAction("ManageGroups", new { Message = ForumMessageId.AddGroupSuccess });
        }

        public ActionResult DeleteGroup(int ID)
        {
            var db = new ApplicationDbContext();
            var group = db.Groups.Where(u => u.GroupID == ID).FirstOrDefault();
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("ManageGroups", new { Message = ForumMessageId.DeleteGroupSuccess });
        }

        // GET: Forum
        [AllowAnonymous]
        public ActionResult Index(int? page)
        {
            var db = new ApplicationDbContext();
            var groups = db.Groups.OrderBy(u => u.GroupName).ToList();

            var currentPage = page ?? 1;
            var model = new ForumIndexViewModel
            {
                Groups = groups.ToPagedList(currentPage, DefaultPageSize)
            };
            return View(model);
        }

        //GET: Forum/ViewGroup/{Int}
        [AllowAnonymous]
        public ActionResult ViewGroup(int? page, int ID)
        {
            var db = new ApplicationDbContext();
            var threads = db.Threads.Where(u => u.GroupID == ID).OrderBy(u => u.TimeDate).ToList();
            var group = db.Groups.Where(u => u.GroupID == ID).FirstOrDefault();

            var currentPage = page ?? 1;
            var model = new ViewGroupViewModel
            {
                Group = group,
                Threads = threads.ToPagedList(currentPage, DefaultPageSize)
            };
            return View(model);
        }

        //GET: Forum/AddThread/{int}
        public ActionResult AddThread(int ID)
        {
            var model = new AddThreadViewModel
            {
                GroupID = ID
            };
            return View(model);
        }

        //POST: Forum/AddThread
        [HttpPost]
        public ActionResult AddThread(AddThreadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var thread = new Thread
            {
                Title = model.Title,
                GroupID = model.GroupID,
                Message = model.Message,
                TimeDate = DateTime.Now
            };
            var db = new ApplicationDbContext();
            var result = db.Threads.Add(thread);
            db.SaveChanges();
            return RedirectToAction("ViewThread", new { ID = result.ThreadID });
        }

        //GET: Forum/ViewThread/{int}
        public ActionResult ViewThread(int? page, int ID)
        {
            var db = new ApplicationDbContext();
            var posts = db.Posts.Where(u => u.ThreadID == ID).OrderBy(u => u.TimeDate).ToList();
            var thread = db.Threads.Where(u => u.ThreadID == ID).FirstOrDefault();
            var currentPage = page ?? 1;
            var model = new ViewThreadViewModel
            {
                Thread = thread,
                Posts = posts.ToPagedList(currentPage, DefaultPageSize)
            };
            return View(model);
        }

        //GET: Forum/AddPost/{int}
        public ActionResult AddPost(int ID)
        {
            var db = new ApplicationDbContext();
            var thread = db.Threads.Where(u => u.ThreadID == ID).FirstOrDefault();
            var model = new AddPostViewModel
            {
                Thread = thread,
                ThreadID = ID
            };
            return View(model);
        }

        //POST: Forum/AddPost/
        [HttpPost]
        public ActionResult AddPost(AddPostViewModel model)
        {
            var db = new ApplicationDbContext();
            var post = new Post
            {
                UserID = User.Identity.GetUserId(),
                TimeDate = DateTime.Now,
                ThreadID = model.ThreadID,
                Message = model.Message
            };
            db.Posts.Add(post);
            db.SaveChanges();
            return RedirectToAction("ViewThread", new { ID = model.ThreadID });
        }

        public enum ForumMessageId
        {
            AddGroupSuccess,
            DeleteGroupSuccess
        }
    }
}