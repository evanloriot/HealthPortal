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
                : "";

            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var apts = db.Appointments.Where(u => u.PatientID == userId && DateTime.Compare(u.TimeDate, DateTime.Today) >= 0).OrderBy(u => u.TimeDate).ToList();
            Appointments = apts;

            var model = new AppointmentIndexViewModel
            {
                Appointment = Appointments.First()
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
            var apts = db.Appointments.Where(u => u.PatientID == userId).OrderBy(u => u.TimeDate).ToList();
            Appointments = apts;

            int currentPageIndex = page ?? 1;
            var model = new ViewAppointmentsViewModel
            {
                Appointments = Appointments.ToPagedList(currentPageIndex, DefaultPageSize)
            };

            return View(model);
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
            DeleteAppointmentSuccess
        }

    }
}