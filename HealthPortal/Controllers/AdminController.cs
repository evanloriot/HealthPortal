using HealthPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HealthPortal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private const int DefaultPageSize = 10;
        private IList<AdminIndexViewModel> users;

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminController()
        {
            this.users = new List<AdminIndexViewModel>();
            var context = new ApplicationDbContext();
            var users = context.Users.ToList();
            foreach (var name in users)
            {
                this.users.Add(new AdminIndexViewModel { ID = name.Id, FullName = name.Identifier.FullName });
            }
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? page, AdminMessageId? message)
        {
            ViewBag.StatusMessage =
                message == AdminMessageId.ManageRoleSuccess ? "You have successfully changed user role."
                : "";

            bool result = User.IsInRole("Admin");
            if (!result)
            {
                ViewBag.Error = "Please sign into an administrative account to access admin functions.";
            }

            int currentPageIndex = page ?? 1;
            return View(this.users.ToPagedList(currentPageIndex, DefaultPageSize));
        }

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult ManageRoles(string UserID)
        {
            if(UserID == null)
            {
                return View("Error");
            }
            bool result = User.IsInRole("Admin");
            if (!result)
            {
                ViewBag.Error = "Please sign into an administrative account to access admin functions.";
            }
            var context = new ApplicationDbContext();
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            var roles = roleManager.Roles.ToList();
            Hashtable ht = new Hashtable();
            foreach (var item in roles)
            {
                ht.Add(item.Name, UserManager.IsInRole(UserID, item.Name));
            }
            ht.Add("UserID", UserID);
            return View(ht);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ManageRoles()
        {
            string UserID = Request.Form["UserID"];
            using (var context = new ApplicationDbContext())
            {

                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);
                var roles = roleManager.Roles.ToList();
                foreach (var item in roles)
                {
                    if (Request.Form.AllKeys.Contains(item.Name))
                    {
                        if (!userManager.IsInRole(UserID, item.Name))
                        {
                            var roleResult = await userManager.AddToRoleAsync(UserID, item.Name);

                            if (roleResult.Succeeded)
                            {
                                continue;
                            }
                            AddErrors(roleResult);
                        }
                    }
                    else
                    {
                        if(userManager.IsInRole(UserID, item.Name))
                        {
                            var roleResult = await userManager.RemoveFromRoleAsync(UserID, item.Name);

                            if (roleResult.Succeeded)
                            {
                                continue;
                            }
                            AddErrors(roleResult);
                        }
                    }
                }
                context.SaveChanges();
            }
            await SignInManager.SignInAsync(UserManager.FindById(User.Identity.GetUserId()), false, false);

            if (!UserManager.IsInRole(User.Identity.GetUserId(), "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", new { UserID = this.users.ToPagedList(1, DefaultPageSize), Message = AdminMessageId.ManageRoleSuccess });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public enum AdminMessageId
        {
            ManageRoleSuccess
        }
    }
}