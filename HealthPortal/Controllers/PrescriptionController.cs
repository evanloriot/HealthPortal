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
    public class PrescriptionController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public PrescriptionController()
        {
        }

        public PrescriptionController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Prescription
        public ActionResult Index()
        {
            if (User.IsInRole("Patient"))
            {
                var userID = User.Identity.GetUserId();
                var db = new ApplicationDbContext();
                var dms = db.PrescriptionMap.Where(u => u.UserID == userID).ToList();

                List<Prescriptions> prescriptions = new List<Prescriptions>();

                foreach (var item in dms)
                {
                    prescriptions.Add(item.Prescription);
                }

                var model = new PrescriptionsIndexViewModel
                {
                    Prescriptions = prescriptions
                };

                return View(model);
            }
            else if (User.IsInRole("Doctor"))
            {
                var docID = User.Identity.GetUserId();
                var db = new ApplicationDbContext();
                var docpatients = db.Users.Where(u => u.PrimaryPhysician.Id == docID).ToList();

                var model = new PrescriptionsIndexViewModel
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

        public ActionResult ViewPatient(string userID, PrescriptionMessageId? message)
        {
            if (userID == null)
            {
                return View("Error");
            }

            ViewBag.StatusMessage = message == PrescriptionMessageId.DeletePrescriptionFromPatientSuccess ? "Prescription successfully removed from patient."
                : "";

            var db = new ApplicationDbContext();
            var dms = db.PrescriptionMap.Where(u => u.UserID == userID).ToList();

            var name = db.Users.Where(u => u.Id == userID).FirstOrDefault().Identifier.FullName;

            List<Prescriptions> prescriptions = new List<Prescriptions>();
            foreach (var item in dms)
            {
                prescriptions.Add(item.Prescription);
            }

            var model = new ViewPatientPrescriptionViewModel
            {
                Prescriptions = prescriptions,
                PatientName = name,
                PatientID = userID
            };

            return View(model);
        }

        public ActionResult ViewPrescriptions(PrescriptionMessageId? message)
        {
            ViewBag.StatusMessage = message == PrescriptionMessageId.DeletePrescriptionSuccess ? "Prescription successfully deleted."
                : message == PrescriptionMessageId.DeletePrescriptionFailure ? "Prescription cannot be deleted as some patients still have this prescription."
                : "";

            var db = new ApplicationDbContext();
            var model = new ViewPrescriptionsViewModel
            {
                Prescriptions = db.Prescriptions.ToList()
            };
            return View(model);
        }

        public ActionResult DeletePrescription(int? ID)
        {
            if (ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var remove = db.Prescriptions.Where(u => u.PrescriptionID == ID).FirstOrDefault();
            db.Prescriptions.Remove(remove);
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                return RedirectToAction("ViewPrescriptions", new { message = PrescriptionMessageId.DeletePrescriptionFailure });
            }
            return RedirectToAction("ViewPrescriptions", new { message = PrescriptionMessageId.DeletePrescriptionSuccess });
        }

        public ActionResult DeletePrescriptionFromPatient(string userID, int? ID)
        {
            if (ID == null || userID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var remove = db.PrescriptionMap.Where(u => u.UserID == userID && u.PrescriptionID == ID).FirstOrDefault();
            db.PrescriptionMap.Remove(remove);
            db.SaveChanges();
            return RedirectToAction("ViewPatient", new { userID, message = PrescriptionMessageId.DeletePrescriptionSuccess });
        }

        public ActionResult AddPrescriptionType()
        {
            var model = new AddPrescriptionTypeViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddPrescriptionType(AddPrescriptionTypeViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var db = new ApplicationDbContext();
            var type = new PrescriptionType
            {
                Type = model.Type
            };
            db.PrescriptionTypes.Add(type);
            db.SaveChanges();
            return RedirectToAction("AddPrescription");
        }

        public ActionResult ManagePrescriptionTypes(PrescriptionMessageId? message)
        {
            ViewBag.StatusMessage = message == PrescriptionMessageId.DeletePrescriptionTypeSuccess ? "Prescription type successfully deleted."
                : message == PrescriptionMessageId.DeletePrescriptionTypeFailure ? "Prescription type cannot be deleted because prescriptions exist with this type."
                : "";
            var db = new ApplicationDbContext();
            var model = new ManagePrescriptionTypesViewModel
            {
                Types = db.PrescriptionTypes.ToList()
            };
            return View(model);
        }

        public ActionResult DeletePrescriptionType(int? ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var type = db.PrescriptionTypes.Where(u => u.PrescriptionTypeID == ID).FirstOrDefault();
            db.PrescriptionTypes.Remove(type);
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                return RedirectToAction("ManagePrescriptionTypes", new { message = PrescriptionMessageId.DeletePrescriptionTypeFailure });
            }
            return RedirectToAction("ManagePrescriptionTypes", new { message = PrescriptionMessageId.DeletePrescriptionTypeSuccess });
        }

        public ActionResult AddPrescription()
        {
            var db = new ApplicationDbContext();
            var model = new AddPrescriptionViewModel
            {
                Types = db.PrescriptionTypes.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AddPrescription(AddPrescriptionViewModel model)
        {
            var db = new ApplicationDbContext();
            if (!ModelState.IsValid)
            {
                model.Types = db.PrescriptionTypes.ToList();
                return View(model);
            }

            var p = new Prescriptions
            {
                PrescriptionName = model.PrescriptionName,
                PrescriptionTypeID = model.PrescriptionTypeID
            };

            db.Prescriptions.Add(p);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult AddPrescriptionToPatient(string patientID)
        {

            if (patientID == null)
            {
                return View("Error");
            }

            var db = new ApplicationDbContext();

            var non = from map in db.PrescriptionMap
                      join prescription in db.Prescriptions on map.PrescriptionID equals prescription.PrescriptionID
                      where map.UserID == patientID
                      select prescription;

            var p = db.Prescriptions.ToList().Except(non).ToList();

            var name = db.Users.Where(u => u.Id == patientID).FirstOrDefault().Identifier.FullName;

            var model = new AddPrescriptionToPatientViewModel
            {
                Prescriptions = p,
                PatientName = name,
                PatientID = patientID,
            };


            return View(model);
        }

        [HttpPost]
        public ActionResult AddPrescriptionToPatient(AddPrescriptionToPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var db = new ApplicationDbContext();

            var patientPrescription = new PrescriptionsMap
            {
                UserID = model.PatientID,
                PrescriptionID = model.PrescriptionID
            };

            db.PrescriptionMap.Add(patientPrescription);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public enum PrescriptionMessageId
        {
            DeletePrescriptionSuccess,
            DeletePrescriptionTypeSuccess,
            DeletePrescriptionTypeFailure,
            DeletePrescriptionFailure,
            DeletePrescriptionFromPatientSuccess
        }
    }
}