﻿using HealthPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HealthPortal.Controllers
{
    [Authorize]
    public class PhysicianController : Controller
    {
        private const int DefaultPageSize = 10;

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public PhysicianController()
        {
        }

        public PhysicianController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Physician
        public ActionResult Index(int? page)
        {
            if (!User.IsInRole("Doctor"))
            {
                ViewBag.Status = "Please sign into a physician account.";
                return View();
            }
            var db = new ApplicationDbContext();
            var userID = User.Identity.GetUserId();
            var patients = db.Users.Where(u => u.PhysicianID == userID).OrderBy(u => u.Identifier.FullName).ToList();

            var currentPage = page ?? 1;
            var model = new PhysicianIndexViewModel
            {
                Patients = patients.ToPagedList(currentPage, DefaultPageSize)
            };
            return View(model);
        }

        public ActionResult ViewPatient(string ID)
        {
            if(ID == null)
            {
                return View("Error");
            }
            var db = new ApplicationDbContext();
            var Patient = db.Users.Where(u => u.Id == ID).FirstOrDefault();
            var MedicalHistory = db.MedicalHistory.Where(u => u.UserID == ID).ToList();
            var Diagnoses = from diagnosisMap in db.DiagnosisMap
                            join diagnosis in db.Diagnoses on diagnosisMap.DiagnosisID equals diagnosis.DiagnosisID
                            where diagnosisMap.UserID == ID
                            select diagnosis;
            var Prescriptions = from prescriptionMap in db.PrescriptionMap
                                join prescriptions in db.Prescriptions on prescriptionMap.PrescriptionID equals prescriptions.PrescriptionID
                                join prescriptionType in db.PrescriptionTypes on prescriptions.PrescriptionTypeID equals prescriptionType.PrescriptionTypeID
                                where prescriptionMap.UserID == ID
                                select prescriptions;
            var model = new ViewPatientViewModel
            {
                Patient = Patient,
                MedicalHistory = MedicalHistory,
                Diagnoses = Diagnoses.ToList(),
                Prescriptions = Prescriptions.ToList()
            };
            return View(model);
        }

        public ActionResult ViewPatientDiagnosisBreakdown(string PhysicianGroupID, string PhysicianSelectID)
        {
            var db = new ApplicationDbContext();
            var role = (from r in db.Roles where r.Name == "Doctor" select r.Id).SingleOrDefault();
            var physicians = db.Identifiers.Where(u => u.User.Roles.Any(r => r.RoleId == role)).ToList();
            IQueryable<DiagnosisGrouping> results;
            if(PhysicianGroupID == null)
            {
                results = from diagnosisMap in db.DiagnosisMap
                              join diagnosis in db.Diagnoses on diagnosisMap.DiagnosisID equals diagnosis.DiagnosisID
                              join user in db.Users on diagnosisMap.UserID equals user.Id
                              group diagnosis by diagnosis.DiagnosisName into d
                              select new DiagnosisGrouping { DiagnosisName = d.Key, Percent = Math.Round((double)100 * d.Count() / db.Users.Count(), 2) };
            }
            else
            {
                var physID = physicians.Where(u => u.IdentifierID == PhysicianGroupID).FirstOrDefault().IdentifierID;

                var divisor = db.Users.Where(u => u.PhysicianID == physID).Count();

                results = from diagnosisMap in db.DiagnosisMap
                              join diagnosis in db.Diagnoses on diagnosisMap.DiagnosisID equals diagnosis.DiagnosisID
                              join user in db.Users on diagnosisMap.UserID equals user.Id
                              where user.PhysicianID == physID
                              group diagnosis by diagnosis.DiagnosisName into d
                              select new DiagnosisGrouping { DiagnosisName = d.Key, Percent = Math.Round((double)100 * d.Count() / divisor, 2) };
            }
            

            DateTime begin = db.Appointments.OrderBy(u => u.TimeDate).FirstOrDefault().TimeDate;
            DateTime middle = begin.AddDays((DateTime.Today - begin).Days);

            DbRawSqlQuery<decimal?> result;
            if(PhysicianSelectID == null)
            {
                result = db.Database.SqlQuery<decimal?>(
                @"declare @firstDate Date;
                set @firstDate=(select top 1 TimeDate from Appointments order by TimeDate);
                declare @endDate Date;
                set @endDate=(select top 1 TimeDate from Appointments order by TimeDate desc);
                declare @medDate Date;
                set @medDate=DATEADD(DAY, DATEDIFF(DAY, @firstDate, @endDate) / 2, @firstDate);

                select 
	                ROUND(AVG((CAST(p2.part as decimal) / p2.total * 100) - (CAST(p1.part as decimal) / p1.total * 100)) * 100, 2)
                from(
	                select 
		                Appointments.PatientID,
		                sum(CAST(CheckUpResponse.Q7A as int)) as part,
		                9*count(CheckUpResponse.Q7A) as total
	                from CheckUpResponse
	                inner join Appointments on CheckUpResponse.AppointmentID = Appointments.AppointmentID
	                where Appointments.TimeDate <= @medDate
	                group by Appointments.PatientID
                ) p1 inner join (
	                select 
		                Appointments.PatientID,
		                sum(CAST(CheckUpResponse.Q7A as int)) as part,
		                9*count(CheckUpResponse.Q7A) as total
	                from CheckUpResponse
	                inner join Appointments on CheckUpResponse.AppointmentID = Appointments.AppointmentID
	                where Appointments.TimeDate > @medDate
	                group by Appointments.PatientID
                ) p2 on p1.PatientID = p2.PatientID");
            }
            else
            {
                result = db.Database.SqlQuery<decimal?>(
                    @"declare @physician nvarchar(128);
                    set @physician='" + PhysicianSelectID + @"';

                    declare @firstDate Date;
                    set @firstDate=(select top 1 TimeDate from Appointments order by TimeDate);
                    declare @endDate Date;
                    set @endDate=(select top 1 TimeDate from Appointments order by TimeDate desc);
                    declare @medDate Date;
                    set @medDate=DATEADD(DAY, DATEDIFF(DAY, @firstDate, @endDate) / 2, @firstDate);

                    select 
	                    ROUND(AVG((CAST(p2.part as decimal) / p2.total * 100) - (CAST(p1.part as decimal) / p1.total * 100)) * 100, 2)
                    from(
	                    select 
		                    Appointments.PatientID,
		                    sum(CAST(CheckUpResponse.Q7A as int)) as part,
		                    9*count(CheckUpResponse.Q7A) as total
	                    from CheckUpResponse
	                    inner join Appointments on CheckUpResponse.AppointmentID = Appointments.AppointmentID
	                    inner join AspNetUsers on Appointments.PatientID = AspNetUsers.Id
	                    where Appointments.TimeDate <= @medDate and AspNetUsers.PhysicianID = @physician
	                    group by Appointments.PatientID
                    ) p1 inner join (
	                    select 
		                    Appointments.PatientID,
		                    sum(CAST(CheckUpResponse.Q7A as int)) as part,
		                    9*count(CheckUpResponse.Q7A) as total
	                    from CheckUpResponse
	                    inner join Appointments on CheckUpResponse.AppointmentID = Appointments.AppointmentID
	                    inner join AspNetUsers on Appointments.PatientID = AspNetUsers.Id
	                    where Appointments.TimeDate > @medDate and AspNetUsers.PhysicianID = @physician
	                    group by Appointments.PatientID
                    ) p2 on p1.PatientID = p2.PatientID");
            }

            var model = new ViewPatientDiagnosisBreakdownViewModel
            {
                Physicians = physicians,
                Rows = results.ToList(),
                Shift = result.FirstOrDefault(),
                Selected = PhysicianSelectID,
                Group = PhysicianGroupID
            };
            return View(model);
        }
    }
}
