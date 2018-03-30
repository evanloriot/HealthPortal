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
    public class AppointmentController : Controller
    {
        private const int DefaultPageSize = 10;
        private IList<Appointments> Appointments;

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AppointmentController()
        {
        }

        public AppointmentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Appointment
        public ActionResult Index(AppointmentMessageId? message)
        {
            ViewBag.StatusMessage =
                message == AppointmentMessageId.ScheduleAppointmentSuccess ? "Your appointment has been scheduled."
                : message == AppointmentMessageId.DeleteAppointmentSuccess ? "Your appointment has been canceled."
                : message == AppointmentMessageId.AddResponseSuccess ? "Your response has been recorded."
                : "";

            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var apts = db.Appointments.Where(u => u.PatientID == userId && DateTime.Compare(u.TimeDate, DateTime.Today) >= 0).OrderBy(u => u.TimeDate).ToList();
            Appointments = apts;

            Appointments apt = null;
            if (User.IsInRole("Doctor"))
            {
                apt = db.Appointments.Where(u => u.PhysicianID == userId && DateTime.Compare(u.TimeDate, DateTime.Today) >= 0).OrderBy(u => u.TimeDate).FirstOrDefault();
            }

            var model = new AppointmentIndexViewModel
            {
                Appointment = Appointments.FirstOrDefault(),
                PatientAppointment = apt
            };

            return View(model);
        }

        // GET: Appointment/ScheduleAppointment
        public ActionResult ScheduleAppointment()
        {
            DateTime Date = DateTime.Now;

            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);

            var physicianId = user.PhysicianID;
            if(physicianId == null)
            {
                return RedirectToAction("Index", "Manage", new { Message = ManageController.ManageMessageId.RequirePhysician });
            }

            var apts = db.Appointments.Where(u => u.PhysicianID == physicianId).OrderBy(u => u.TimeDate).ToList();

            Dictionary<string, bool> TimeDates = new Dictionary<string, bool>();
            foreach(var item in apts)
            {
                TimeDates[item.TimeDate.ToString()] = true;
            }

            var model = new ScheduleAppointmentViewModel
            {
                PhysicianAppointments = apts,
                Date = Date,
                TimeDates = TimeDates
            };

            return View(model);
        }

        // POST: Appointment/ScheduleAppointment 
        [HttpPost]
        public ActionResult ScheduleAppointment(ScheduleAppointmentViewModel model)
        {
            string reason = Request.Form["reas"];
            string date = Request.Form["value"];
            DateTime TimeDate = Convert.ToDateTime(date);

            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);

            var physicianId = user.PhysicianID;

            db.Appointments.Add(new Appointments
            {
                PatientID = userId,
                PhysicianID = physicianId,
                Reason = reason,
                TimeDate = TimeDate
            });
            db.SaveChanges();

            return RedirectToAction("Index", new { Message = AppointmentMessageId.ScheduleAppointmentSuccess } );
        }

        // GET: Appointment/ViewAppointments
        public ActionResult ViewAppointments(int? page)
        {
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Doctor"))
            {
                Appointments = db.Appointments.Where(u => u.PhysicianID == userId).OrderBy(u => u.TimeDate).ToList();
            }
            else
            {
                Appointments = db.Appointments.Where(u => u.PatientID == userId).OrderBy(u => u.TimeDate).ToList();
            }

            int currentPageIndex = page ?? 1;
            var model = new ViewAppointmentsViewModel
            {
                Appointments = Appointments.ToPagedList(currentPageIndex, DefaultPageSize)
            };

            return View(model);
        }
        
        // GET: Appointment/AddResponse
        public ActionResult AddResponse(int ID = -1)
        {
            if(ID == -1)
            {
                return RedirectToAction("Index");
            }

            var db = new ApplicationDbContext();
            var r = db.CheckUpResponse.Where(u => u.AppointmentID == ID).FirstOrDefault();
            var apt = db.Appointments.Where(u => u.AppointmentID == ID).FirstOrDefault();

            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);

            var physicianId = user.PhysicianID;
            var physician = UserManager.FindById(physicianId);
            string physicianName = physician == null ? null : "Dr. " + physician.Identifier.FullName;

            AddResponseViewModel model;
            if(r != null)
            {
                model = new AddResponseViewModel
                {
                    AppointmentID = ID,
                    Appointment = apt,
                    PhysicianName = physicianName,
                    Q1A = r.Q1A,
                    Q2A = r.Q2A,
                    Q3A = r.Q3A,
                    Q4A = r.Q4A,
                    Q5A = r.Q5A,
                    Q6A = r.Q6A,
                    Q7A = r.Q7A,
                    Q8A = r.Q8A,
                    Q9A = r.Q9A,
                    Q10A = r.Q10A
                };
            }
            else
            {
                model = new AddResponseViewModel
                {
                    AppointmentID = ID,
                    Appointment = apt,
                    PhysicianName = physicianName,
                };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddResponse(AddResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            CheckUpResponse response = new CheckUpResponse
            {
                AppointmentID = model.AppointmentID,
                Q1A = model.Q1A,
                Q2A = model.Q2A,
                Q3A = model.Q3A,
                Q4A = model.Q4A,
                Q5A = model.Q5A,
                Q6A = model.Q6A,
                Q7A = model.Q7A,
                Q8A = model.Q8A,
                Q9A = model.Q9A,
                Q10A = model.Q10A
            };
            using(var db = new ApplicationDbContext())
            {
                db.CheckUpResponse.Add(response);
                db.SaveChanges();
            }
            
            return RedirectToAction("Index", new { Message = AppointmentMessageId.AddResponseSuccess });
        }

        public ActionResult DeleteAppointment()
        {
            int id = Convert.ToInt32(Request.Form["ID"]);
            var db = new ApplicationDbContext();
            var apt = db.Appointments.Where(u => u.AppointmentID == id).FirstOrDefault();
            db.Appointments.Remove(apt);
            db.SaveChanges();
            return RedirectToAction("Index", new { Message = AppointmentMessageId.DeleteAppointmentSuccess });
        }

        public enum AppointmentMessageId
        {
            ScheduleAppointmentSuccess,
            AddResponseSuccess,
            DeleteAppointmentSuccess
        }

    }
}