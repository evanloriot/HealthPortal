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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var group = new Group
            {
                GroupName = model.GroupName
            };
            var db = new ApplicationDbContext();
            db.Groups.Add(group);
            db.SaveChanges();
            return RedirectToAction("ManageGroups", new { Message = ForumMessageId.AddGroupSuccess });
        }

        //GET: Forum/DeleteGroup
        public ActionResult DeleteGroup(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
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
        public ActionResult ViewGroup(int? ID, int? page)
        {
            if(ID == null)
            {
                return View("Error");
            }
            string search = null;
            if(Request.Form["SearchString"] != null && Request.Form["SearchString"] != "")
            {
                search = Request.Form["SearchString"];
            }
            var db = new ApplicationDbContext();
            List<Thread> threads = new List<Thread>();
            if (search != null)
            {
                var results = from thread in db.Threads
                              join post in db.Posts on thread.ThreadID equals post.ThreadID
                              where thread.GroupID == ID && (thread.Title.ToLower().Contains(search.ToLower()) || post.Message.Contains(search.ToLower()) || thread.Message.Contains(search.ToLower()))
                              orderby thread.TimeDate descending
                              select thread;
                threads = results.ToList();
            }
            else
            {
                threads = db.Threads.Where(u => u.GroupID == ID).OrderByDescending(u => u.TimeDate).ToList();
            }
            var group = db.Groups.Where(u => u.GroupID == ID).FirstOrDefault();

            var currentPage = page ?? 1;
            var model = new ViewGroupViewModel
            {
                Search = search,
                Group = group,
                Threads = threads.ToPagedList(currentPage, DefaultPageSize)
            };
            return View(model);
        }

        //GET: Forum/AddThread/{int}
        public ActionResult AddThread(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var model = new AddThreadViewModel
            {
                GroupID = (int) ID
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
                UserID = User.Identity.GetUserId(),
                Title = model.Title,
                GroupID = model.GroupID,
                Message = model.Message == null? "" : model.Message,
                TimeDate = DateTime.Now
            };
            var db = new ApplicationDbContext();
            var result = db.Threads.Add(thread);
            db.SaveChanges();
            return RedirectToAction("ViewThread", new { ID = result.ThreadID });
        }

        //GET: Forum/DeleteThread{int, int}
        public ActionResult DeleteThread(int? ID, int? GroupID)
        {
            if(ID == null || GroupID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var thread = db.Threads.Where(u => u.ThreadID == ID).FirstOrDefault();
            db.Threads.Remove(thread);
            db.SaveChanges();
            return RedirectToAction("ViewGroup", new { ID = GroupID });
        }

        //GET: Forum/ViewThread/{int}
        public ActionResult ViewThread(int? page, int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
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
        public ActionResult AddPost(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var thread = db.Threads.Where(u => u.ThreadID == ID).FirstOrDefault();
            var model = new AddPostViewModel
            {
                Thread = thread,
                ThreadID = (int) ID
            };
            return View(model);
        }

        //POST: Forum/AddPost/
        [HttpPost]
        public ActionResult AddPost(AddPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var db = new ApplicationDbContext();
            var post = new Post
            {
                UserID = User.Identity.GetUserId(),
                TimeDate = DateTime.Now,
                ThreadID = model.ThreadID,
                Message = model.Message,
                Deleted = false
            };
            db.Posts.Add(post);
            db.SaveChanges();
            return RedirectToAction("ViewThread", new { ID = model.ThreadID });
        }

        //GET: Forum/DeletePost/{int}
        public ActionResult DeletePost(int? ID, int? ThreadID)
        {
            if(ID == null || ThreadID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var post = db.Posts.Where(u => u.PostID == ID).FirstOrDefault();
            post.Deleted = true;
            db.SaveChanges();
            return RedirectToAction("ViewThread", new { ID = ThreadID });
        }

        public enum ForumMessageId
        {
            AddGroupSuccess,
            DeleteGroupSuccess
        }
    }
}