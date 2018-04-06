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
            if(User.IsInRole("Patient"))
            {
                var userID = User.Identity.GetUserId();
                var db = new ApplicationDbContext();
                var dms = db.DiagnosisMap.Where(u => u.UserID == userID).ToList();

                List<Diagnosis> diagnoses = new List<Diagnosis>();
                foreach (var item in dms)
                {
                    diagnoses.Add(item.Diagnosis);
                }

                var model = new DiagnosisIndexViewModel
                {
                    Diagnoses = diagnoses
                };

                return View(model);
            }
            else if(User.IsInRole("Doctor"))
            {
                var docID = User.Identity.GetUserId();
                var db = new ApplicationDbContext();
                var docpatients = db.Users.Where(u => u.PrimaryPhysician.Id == docID).ToList();

                var model = new DiagnosisIndexViewModel
                {
                    PatientList = docpatients
                };

                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult ViewDiagnoses(DiagnosisMessageId? message)
        {
            ViewBag.StatusMessage = message == DiagnosisMessageId.DeleteDiagnosisSuccess ? "Diagnosis successfully deleted."
                : message == DiagnosisMessageId.DeleteDiagnosisFailure ? "Diagnosis cannot be deleted as some patients still have this diagnosis."
                : "";

            var db = new ApplicationDbContext();
            var model = new ViewDiagnosesViewModel
            {
                Diagnoses = db.Diagnoses.ToList()
            };
            return View(model);
        }

        public ActionResult DeleteDiagnosis(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var remove = db.Diagnoses.Where(u => u.DiagnosisID == ID).FirstOrDefault();
            db.Diagnoses.Remove(remove);
            try
            {
                db.SaveChanges();
            }
            catch(Exception)
            {
                return RedirectToAction("ViewDiagnoses", new { message = DiagnosisMessageId.DeleteDiagnosisFailure });
            }
            return RedirectToAction("ViewDiagnoses", new { message = DiagnosisMessageId.DeleteDiagnosisSuccess });
        }

        public ActionResult ViewPatient(string userID, DiagnosisMessageId? message)
        {
            if(userID == null)
            {
                return View("Error");
            }

            ViewBag.StatusMessage = message == DiagnosisMessageId.DeleteDiagnosisFromPatientSuccess ? "Diagnosis successfully removed from patient."
                : "";

            var db = new ApplicationDbContext();
            var dms = db.DiagnosisMap.Where(u => u.UserID == userID).ToList();

            var name = db.Users.Where(u => u.Id == userID).FirstOrDefault().Identifier.FullName;

            List<Diagnosis> diagnoses = new List<Diagnosis>();
            foreach (var item in dms)
            {
                diagnoses.Add(item.Diagnosis);
            }

            var model = new ViewPatientDiagnosisViewModel
            {
                Diagnoses = diagnoses,
                PatientName = name,
                PatientID = userID
            };

            return View(model);
        }

        public ActionResult DeleteDiagnosisFromPatient(string userID, int? ID)
        {
            if(ID == null || userID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var remove = db.DiagnosisMap.Where(u => u.UserID == userID && u.DiagnosisID == ID).FirstOrDefault();
            db.DiagnosisMap.Remove(remove);
            db.SaveChanges();
            return RedirectToAction("ViewPatient", new { userID, message = DiagnosisMessageId.DeleteDiagnosisSuccess });
        }

        public ActionResult AddDiagnosis()
        {
            var model = new AddDiagnosisViewModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddDiagnosis(AddDiagnosisViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var db = new ApplicationDbContext();

            var diagnosis = new Diagnosis
            {
                DiagnosisName = model.DiagnosisName
            };

            db.Diagnoses.Add(diagnosis);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult AddDiagnosisToPatient(string patientID)
        {

            if (patientID == null)
            {
                return View("Error");
            }

            var db = new ApplicationDbContext();

            var non = from map in db.DiagnosisMap
                      join diagnosis in db.Diagnoses on map.DiagnosisID equals diagnosis.DiagnosisID
                      where map.UserID == patientID
                      select diagnosis;

            var d = db.Diagnoses.ToList().Except(non).ToList();

            var name = db.Users.Where(u => u.Id == patientID).FirstOrDefault().Identifier.FullName;

            var model = new AddDiagnosisToPatientViewModel
            {
                Diagnoses = d,
                PatientName = name,
                PatientID = patientID,
            };
            

            return View(model);
        }

        [HttpPost]
        public ActionResult AddDiagnosisToPatient(AddDiagnosisToPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var db = new ApplicationDbContext();

            var patientDiagnosis = new DiagnosisMap
            {
                UserID = model.PatientID,
                DiagnosisID = model.DiagnosisID
            };

            db.DiagnosisMap.Add(patientDiagnosis);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public enum DiagnosisMessageId
        {
            DeleteDiagnosisSuccess,
            DeleteDiagnosisFailure,
            DeleteDiagnosisFromPatientSuccess
        }
    }
}