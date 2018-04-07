﻿using HealthPortal.Models;
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

        public ActionResult AddPrescription()
        {
            var model = new AddPrescriptionViewModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddPrescription(AddPrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var db = new ApplicationDbContext();

            var p = new Prescriptions
            {
                PrescriptionName = model.PrescriptionName
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
            DeletePrescriptionFailure,
            DeletePrescriptionFromPatientSuccess
        }
    }
}