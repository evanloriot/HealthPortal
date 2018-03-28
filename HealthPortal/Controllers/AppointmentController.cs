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
        public ActionResult Index()
        {
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var apts = db.Appointments.Where(u => u.PatientID == userId).OrderBy(u => u.TimeDate).ToList();
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
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var apts = db.Appointments.Where(u => u.PatientID == userId).OrderBy(u => u.TimeDate).ToList();

            var model = new ScheduleAppointmentViewModel
            {
                PhysicianAppointments = apts
            };

            return View(model);
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

    }
}