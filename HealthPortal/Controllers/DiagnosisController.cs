using HealthPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HealthPortal.Controllers
{
    [Authorize]
    public class DiagnosisController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public DiagnosisController()
        {
        }

        public DiagnosisController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Diagnosis
        public ActionResult Index()
        {
            var userID = User.Identity.GetUserId();
            var db = new ApplicationDbContext();
            var dms = db.DiagnosisMap.Where(u => u.UserID == userID).ToList();

            List<Diagnosis> diagnoses = new List<Diagnosis>();
            foreach(var item in dms)
            {
                diagnoses.Add(item.Diagnosis);
            }

            var model = new DiagnosisIndexViewModel
            {
                Diagnoses = diagnoses
            };

            return View(model);
        }
    }
}