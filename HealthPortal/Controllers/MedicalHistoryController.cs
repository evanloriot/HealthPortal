using HealthPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HealthPortal.Controllers
{
    [Authorize]
    public class MedicalHistoryController : Controller
    {
        private const int DefaultPageSize = 10;
        private IList<ApplicationUser> Patients = new List<ApplicationUser>();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public MedicalHistoryController()
        {
        }

        public MedicalHistoryController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: MedicalHistory
        public ActionResult Index(int? page, MedicalHistoryMessageId? message)
        {
            ViewBag.StatusMessage = message == MedicalHistoryMessageId.AddMedicalHistorySuccess ? "Medical record added."
                : message == MedicalHistoryMessageId.EditMedicalHistorySuccess ? "Medical record changed."
                : message == MedicalHistoryMessageId.DeleteMedicalHistorySuccess ? "Medical record deleted."
                : "";

            var userId = User.Identity.GetUserId();
            var db = new ApplicationDbContext();
            var MedicalHistory = db.MedicalHistory.Where(u => u.UserID == userId).ToList();
            
            if (User.IsInRole("Doctor"))
            {
                Patients = db.Users.Where(u => u.PhysicianID == userId).OrderBy(u => u.Identifier.FullName).ToList();
            }
            Dictionary<string, List<MedicalHistory>> patientHistory = new Dictionary<string, List<MedicalHistory>>();
            if(Patients.Count != 0)
            {
                foreach (var user in Patients)
                {
                    patientHistory[user.Id] = db.MedicalHistory.Where(u => u.UserID == user.Id).ToList();
                }
            }

            int currentPageIndex = page ?? 1;
            var model = new MedicalHistoryIndexViewModel
            {
                History = MedicalHistory,
                Patients = Patients.ToPagedList(currentPageIndex, DefaultPageSize),
                PatientHistories = patientHistory
            };

            return View(model);
        }

        // GET: MedicalHistory/AddMedicalHistory
        public ActionResult AddMedicalHistory(string ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var model = new AddMedicalHistoryViewModel
            {
                ID = ID
            };

            return View(model);
        }

        // POST: MedicalHistory/AddMedicalHistory
        [HttpPost]
        public ActionResult AddMedicalHistory(AddMedicalHistoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = model.ID;
            var detail = new MedicalHistory
            {
                UserID = userId,
                Details = model.Details
            };

            var db = new ApplicationDbContext();
            db.MedicalHistory.Add(detail);
            db.SaveChanges();
            return RedirectToAction("Index", new { Message = MedicalHistoryMessageId.AddMedicalHistorySuccess } );
        }

        // GET: MedicalHistory/EditMedicalHistory
        public ActionResult EditMedicalHistory(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var detail = db.MedicalHistory.Where(u => u.MedicalHistoryID == ID).FirstOrDefault();
            var model = new EditMedicalHistoryViewModel
            {
                ID = (int) ID,
                Details = detail.Details
            };
            return View(model);
        }

        //POST: MedicalHistory/EditMedicalHistory
        [HttpPost]
        public ActionResult EditMedicalHistory(EditMedicalHistoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var db = new ApplicationDbContext();
            var record = db.MedicalHistory.Where(u => u.MedicalHistoryID == model.ID).FirstOrDefault();
            record.Details = model.Details;
            db.Entry(record).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { Message = MedicalHistoryMessageId.EditMedicalHistorySuccess });
        }

        public ActionResult DeleteMedicalHistory(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var record = db.MedicalHistory.Where(u => u.MedicalHistoryID == ID).FirstOrDefault();
            db.MedicalHistory.Remove(record);
            db.SaveChanges();
            return RedirectToAction("Index", new { Message = MedicalHistoryMessageId.DeleteMedicalHistorySuccess });
        }

        public enum MedicalHistoryMessageId
        {
            AddMedicalHistorySuccess,
            EditMedicalHistorySuccess,
            DeleteMedicalHistorySuccess
        }
    }
}