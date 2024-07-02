using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using FMB_CIS.Services;
using FMB_CIS.Utilities;
using Humanizer.Localisation;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Configuration;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Tls;
using Services.Utilities;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Security.Claims;
using System.Security.Cryptography.Xml;

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment WebHostEnvironment;
        private readonly INotificationAbstract _notificationService;
        private readonly IWorkflowAbstract _workflowService;
        private IWebHostEnvironment EnvironmentHosting;

        public void LogUserActivity(string entity, string userAction, string remarks, int userId = 0, string source = "Web", DateTime? apkDateTime = null)
        {
            try
            {
                if (entity.ToUpper() == "LOGOUT"
                    && source.ToUpper() == "WEB")
                {
                    var fullname = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("FullName").Value);
                    remarks = "User logged out. Username: " + fullname;
                }

                int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                //Inserting record to UserActivityLog database
                tbl_user_activitylog activityLog = new tbl_user_activitylog()
                {
                    UserId = uid,
                    Entity = entity,
                    UserAction = (userAction ?? ""),
                    Remarks = (remarks ?? ""),
                    CreatedDt = DateTime.Now.Date,
                    CreatedTimestamp = DateTime.Now,
                    ApkDatetime = apkDateTime,
                    Source = source
                };
                _context.Add(activityLog);
                _context.SaveChanges();

                ////Inserting record to UserActivity Log file
                //var userdata = _userRepository.TableNoTracking.Where(x => x.Id == (_userSession.UserId != 0 ? _userSession.UserId : userId))
                //              .Select(y => y.UserCode + " (" + y.FirstName + ")").FirstOrDefault();
                //Utility.Logger.UserLog("The " + userdata + " In Module " + entity + "-" + (userAction ?? "") + ":" + (remarks ?? ""));


            }
            catch (Exception ex)
            {
            }
        }
        public ApplicationController(IConfiguration configuration,
                                    LocalContext context,
                                    IEmailSender emailSender,
                                    IWebHostEnvironment _environment,
                                    INotificationAbstract notificationService,
                                    IWorkflowAbstract workflowService)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            this.WebHostEnvironment = _environment;
            _notificationService = notificationService;
            _workflowService = workflowService;
            EnvironmentHosting = _environment;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ManageApplications", "Application");
            //return View();

        }

        public IActionResult ManageApplications()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 //where a.tbl_user_id == userID
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id, ReferenceNo = a.ReferenceNo };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_permit_to_resell")]
        public IActionResult ResellPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Permit to Re-sell/Transfer Ownership"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_authority_to_lend")]
        public IActionResult LendPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Authority to Lend"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_certification_of_registration")]
        public IActionResult RegistrationPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Certificate of Registration"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      date_of_expiration = a.date_of_expiration,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count)
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_authority_to_rent")]
        public IActionResult RentPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Authority to Rent"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_authority_to_lease")]
        public IActionResult LeasePermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Authority to Lease"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_permit_to_sell")]
        public IActionResult SellPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Permit to Sell"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_permit_to_purchase")]
        public IActionResult PurchasePermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Permit to Purchase"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,//pS.id,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();

            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_permit_to_import")]
        public IActionResult ImportPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  //join pS in _context.tbl_permit_status on a.status equals pS.id
                                  //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                  join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                  where pT.name == "Permit to Import"
                                  let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                  let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      applicationDate = a.date_created,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      contact = usr.contact_no,
                                      address = usr.street_address,
                                      application_type = appt.name,
                                      permit_type = pT.name,
                                      permit_status = wfs.name,//pS.status,
                                      permit_status_id = a.status,
                                      permit_statuses = wfs.name,
                                      tbl_user_id = (int)usr.id,
                                      status = (int)a.status,
                                      qty = a.qty,
                                      ReferenceNo = a.ReferenceNo,
                                      currentStepCount = current_step_count,
                                      currentMaxCount = current_max_count,
                                      currentPercentage = (current_step_count * 100 / current_max_count),
                                      date_of_expiration = a.date_of_expiration,
                                  }).ToList();
            //foreach (ApplicantListViewModel mod in applicationMod)
            //{
            //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
            //}
            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [Authorize]
        [HttpGet]
        public IActionResult EditApplication(string uid, string appid)
        {
            ViewModel mymodel = new ViewModel();
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //tbl_user user = _context.tbl_user.Find(uid);
            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;
            ViewBag.uid = uid;
            ViewBag.appid = appid;
            if (uid == null || appid == null || loggedUserID != Convert.ToInt32(uid))
            {
                ModelState.AddModelError("", "Invalid Application");
                return RedirectToAction("Index", "Dashboard");
            }

            else
            {
                int usid = Convert.ToInt32(uid);
                int applid = Convert.ToInt32(appid);
                var UserList = _context.tbl_user.Single(i => i.id == usid);

                //var UserInfo = UserList.Where(m => m.id == usid).ToList();

                //ViewModel mymodel = new ViewModel();
                var applicationlist = from a in _context.tbl_application
                                      where a.tbl_user_id == usid && a.id == applid
                                      select a;

                //CODE FOR FILE DOWNLOAD
                int applicID = Convert.ToInt32(appid);

                mymodel.uid = uid;
                mymodel.appid = applicID.ToString();
                //File Paths from Database
                var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == usid && f.is_proof_of_payment != true).ToList();
                List<tbl_files> files = new List<tbl_files>();

                foreach (var fileList in filesFromDB)
                {
                    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                    //files.Add(new tbl_files { filename = f });
                }
                mymodel.tbl_Files = files;

                //FILES FOR PROOF OF PAYMENT
                var filesFromPayment = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && f.is_proof_of_payment == true).ToList();
                List<tbl_files> paymentFiles = new List<tbl_files>();

                foreach (var fileList in filesFromPayment)
                {
                    paymentFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                }
                mymodel.proofOfPaymentFiles = paymentFiles;
                //END FOR FILE DOWNLOAD

                //Document Tagging
                //var fileWithCommentsforDocTagging = (from f in _context.tbl_files
                //                                    //join c in _context.tbl_comments on f.Id equals c.tbl_files_id
                //                                    //join usr in _context.tbl_user on c.created_by equals usr.id
                //                                    where f.tbl_application_id == applicID && f.created_by == usid
                //                                    select new FilesWithComments
                //                                    {
                //                                        tbl_files_id = f.Id,
                //                                        filename = f.filename,
                //                                        tbl_application_id = f.tbl_application_id,
                //                                        tbl_files_status = f.status,
                //                                        //comment = c.comment
                //                                    }).ToList();

                ////Add the latest comments for every file
                //var commentsList = _context.tbl_comments.Where(c => c.tbl_application_id == applicID).ToList();

                //for (int i=0; i< fileWithCommentsforDocTagging.Count; i++)
                //{
                //    var latestComment = commentsList.Where(c => c.tbl_files_id == fileWithCommentsforDocTagging[i].tbl_files_id).LastOrDefault();
                //    if (latestComment != null)
                //    {
                //        fileWithCommentsforDocTagging[i].comment = latestComment.comment;
                //        fileWithCommentsforDocTagging[i].tbl_comments_created_by = latestComment.created_by;
                //    }
                //}

                //mymodel.filesWithComments = fileWithCommentsforDocTagging;
                //End for Document Tagging

                //Get application permit type id
                int permitTypeID = Convert.ToInt32(_context.tbl_application.Where(a => a.id == applicID).Select(a => a.tbl_permit_type_id).FirstOrDefault());

                //Application Group
                //Get all chainsaw registered to application id

                //var applicationGroups = _context.tbl_application_group.Where(
                //                                                    g => g.tbl_application_id == applicID
                //                                                    ).ToList();

                var applicationGroups = (from ag in _context.tbl_application_group
                                         join b in _context.tbl_brands on ag.brand_id equals b.id into brandGroup
                                         from BrandTBL in brandGroup.DefaultIfEmpty()
                                         where ag.tbl_application_id == applicID
                                         select new tbl_application_group
                                         {
                                             id = ag.id,
                                             tbl_application_id = ag.tbl_application_id,
                                             supplier_name = ag.supplier_name,
                                             supplier_address = ag.supplier_address,
                                             expected_time_arrival = ag.expected_time_arrival,
                                             power_source = ag.power_source,
                                             unit_of_measure = ag.unit_of_measure,
                                             brand_id = ag.brand_id,
                                             brand = BrandTBL.name,
                                             model = ag.model,
                                             engine_serialNo = ag.engine_serialNo,
                                             quantity = ag.quantity,
                                             created_by = ag.created_by,
                                             modified_by = ag.modified_by,
                                             date_created = ag.date_created,
                                             date_modified = ag.date_modified
                                         }).ToList();

                mymodel.tbl_Application_Group = applicationGroups;

                //Document Checklist (For New Upload)
                var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == permitTypeID && c.is_active == true).ToList();
                mymodel.tbl_Document_Checklist = myChecklist;
                //End for Document Checklist

                //Document Tagging and Checklist (Displaying uploaded documents)
                //Get uploaded files and requirements
                var fileWithCommentsforDocTagging = (from br in _context.tbl_files_checklist_bridge
                                                     join dc in _context.tbl_document_checklist on br.tbl_document_checklist_id equals dc.id
                                                     join f in _context.tbl_files on br.tbl_files_id equals f.Id
                                                     where dc.permit_type_id == permitTypeID && f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && dc.is_active == true
                                                     select new FilesWithComments
                                                     {
                                                         tbl_document_checklist_id = dc.id,
                                                         tbl_document_checklist_name = dc.name,
                                                         tbl_files_id = f.Id,
                                                         filename = f.filename,
                                                         tbl_application_id = f.tbl_application_id,
                                                         tbl_files_status = br.status,
                                                         bridge_id = br.id
                                                         //comment = c.comment
                                                     }).OrderBy(o => o.filename).ToList();

                var requiredDocumentList = _context.tbl_document_checklist.Where(c => c.permit_type_id == permitTypeID && c.is_active == true).ToList();
                foreach (var reqList in requiredDocumentList)
                {
                    bool isReqAvailable = fileWithCommentsforDocTagging.Any(r => r.tbl_document_checklist_id == reqList.id);
                    if (isReqAvailable == false)
                    {
                        fileWithCommentsforDocTagging.Add(new FilesWithComments
                        {
                            tbl_document_checklist_id = reqList.id,
                            tbl_document_checklist_name = reqList.name,
                            tbl_files_id = null,
                            filename = "N/A",
                            tbl_application_id = applicID,
                            tbl_files_status = "N/A"
                        });
                    }
                }

                //Add the latest comments for every file
                var commentsList = _context.tbl_comments.Where(c => c.tbl_application_id == applicID).ToList();

                for (int i = 0; i < fileWithCommentsforDocTagging.Count; i++)
                {
                    var latestComment = commentsList.Where(c => c.bridge_id == fileWithCommentsforDocTagging[i].bridge_id).LastOrDefault();
                    if (latestComment != null && latestComment.bridge_id != null)
                    {
                        fileWithCommentsforDocTagging[i].comment = latestComment.comment;
                        fileWithCommentsforDocTagging[i].tbl_comments_created_by = latestComment.created_by;
                    }
                }

                mymodel.filesWithComments = fileWithCommentsforDocTagging;
                //End for Document Tagging and Checklist

                var permitTypeOfThisApplication = _context.tbl_application.Where(a => a.id == applid).Select(a => a.tbl_permit_type_id).FirstOrDefault();
                if (permitTypeOfThisApplication == 13) //For Certificate of Registration
                {
                    //HISTORY
                    var applicationtypelist = _context.tbl_application_type;
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          //join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          //join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          join reg in _context.tbl_region on usr.tbl_region_id equals reg.id
                                          join prov in _context.tbl_province on usr.tbl_province_id equals prov.id
                                          join ct in _context.tbl_city on usr.tbl_city_id equals ct.id
                                          join brngy in _context.tbl_brgy on usr.tbl_brgy_id equals brngy.id
                                          join csaw in _context.tbl_chainsaw on a.id equals csaw.tbl_application_id
                                          where a.tbl_user_id == usid && a.id == applid
                                          select new ApplicantListViewModel
                                          {
                                              id = a.id,
                                              tbl_user_id = usid,
                                              full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                              full_address = usr.street_address + " " + brngy.name + " " + ct.name + " " + prov.name + " " + reg.name,
                                              email = usr.email,
                                              permit_type = pT.name,
                                              permit_status = wfs.name, //pS.status,
                                              permit_status_id = a.status,
                                              //permit_statuses = pSs.status,
                                              permit_statuses = wfs.name,
                                              status = Convert.ToInt32(a.status),
                                              //user_type = usrtyps.name,
                                              comment = usr.comment,
                                              qty = a.qty,
                                              specification = a.tbl_specification_id,
                                              initial_date_of_inspection = a.initial_date_of_inspection,
                                              inspectionDate = a.date_of_inspection,
                                              address = usr.street_address,
                                              //expectedTimeArrived = a.expected_time_arrival,
                                              //expectedTimeRelease = a.expected_time_release,
                                              purpose = a.purpose,
                                              date_of_registration = a.date_of_registration,
                                              date_of_expiration = a.date_of_expiration,
                                              chainsawBrand = csaw.Brand,
                                              chainsawModel = csaw.Model,
                                              Engine = csaw.Engine,
                                              powerSource = csaw.Power,
                                              //Watt = csaw.watt,
                                              //hp = csaw.hp,
                                              watt_dec = csaw.watt_dec,
                                              hp_dec = csaw.hp_dec,
                                              gb = csaw.gb,
                                              chainsaw_serial_number = csaw.chainsaw_serial_number,
                                              chainsawSupplier = csaw.supplier,
                                              date_purchase = csaw.date_purchase,
                                              renew_from = a.renew_from,
                                              ReferenceNo = a.ReferenceNo,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic 
                                          }).FirstOrDefault();
                    applicationMod.currentPercentage = (applicationMod.currentStepCount * 100 / applicationMod.currentMaxCount);
                    mymodel.applicantViewModels = applicationMod;


                }
                else
                {
                    //HISTORY
                    var applicationtypelist = _context.tbl_application_type;
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          //join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          //join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          join reg in _context.tbl_region on usr.tbl_region_id equals reg.id
                                          join prov in _context.tbl_province on usr.tbl_province_id equals prov.id
                                          join ct in _context.tbl_city on usr.tbl_city_id equals ct.id
                                          join brngy in _context.tbl_brgy on usr.tbl_brgy_id equals brngy.id
                                          where a.tbl_user_id == usid && a.id == applid
                                          select new ApplicantListViewModel
                                          {
                                              id = a.id,
                                              tbl_user_id = usid,
                                              full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                              full_address = usr.street_address + " " + brngy.name + " " + ct.name + " " + prov.name + " " + reg.name,
                                              email = usr.email,
                                              permit_type = pT.name,
                                              permit_status = wfs.name, //pS.status,
                                              permit_status_id = a.status,
                                              //permit_statuses = pSs.status,
                                              permit_statuses = wfs.name,
                                              status = Convert.ToInt32(a.status),
                                              // user_type = usrtyps.name,
                                              comment = usr.comment,
                                              qty = a.qty,
                                              specification = a.tbl_specification_id,
                                              initial_date_of_inspection = a.initial_date_of_inspection,
                                              inspectionDate = a.date_of_inspection,
                                              address = usr.street_address,
                                              //expectedTimeArrived = a.expected_time_arrival,
                                              //expectedTimeRelease = a.expected_time_release,
                                              purpose = a.purpose,
                                              date_of_registration = a.date_of_registration,
                                              date_of_expiration = a.date_of_expiration,
                                              renew_from = a.renew_from,
                                              ReferenceNo = a.ReferenceNo,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic
                                              //coordinatedWithEnforcementDivision = a.coordinatedWithEnforcementDivision
                                          }).FirstOrDefault();
                    applicationMod.currentPercentage = (applicationMod.currentStepCount * 100 / applicationMod.currentMaxCount);
                    //mymodel.email = UserList.email;
                    mymodel.applicantViewModels = applicationMod;
                    //mymodel.tbl_Users = UserInfo;
                }

                //Proof of Payment
                var paymentDetails = _context.tbl_application_payment.Where(p => p.tbl_application_id == applid).FirstOrDefault();
                mymodel.tbl_Application_Payment = paymentDetails;
                //Check if this application has applied for renewal
                var checkRenewal = _context.tbl_application.Where(a => a.renew_from == applid).FirstOrDefault();
                if (checkRenewal != null)
                {
                    ViewBag.hasRenewal = true;
                    ViewBag.RenewalID = checkRenewal.id;
                }
                else
                {
                    ViewBag.hasRenewal = false;
                }
                //End of checking if this application has applied for renewal

                //Display List of Comments for Application Approval (User to Inspector Conversation)
                mymodel.commentsViewModelsList = (from c in _context.tbl_comments
                                                  where c.tbl_application_id == applid
                                                  //join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                  join usr in _context.tbl_user on c.created_by equals usr.id
                                                  select new CommentsViewModel
                                                  {
                                                      tbl_application_id = c.tbl_application_id.GetValueOrDefault(),
                                                      //tbl_files_id = c.tbl_files_id,
                                                      //fileName = f.filename,
                                                      comment_to = c.comment_to,
                                                      comment = c.comment,
                                                      commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                      created_by = c.created_by,
                                                      modified_by = c.modified_by,
                                                      date_created = c.date_created,
                                                      date_modified = c.date_modified
                                                  }).Where(u => u.comment_to == "User To Inspector").OrderByDescending(d => d.date_created);

                //Display List of Comments for Application Approval (User to CENRO Conversation)
                mymodel.commentsViewModels2ndList = (from c in _context.tbl_comments
                                                     where c.tbl_application_id == applid
                                                     //join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                     join usr in _context.tbl_user on c.created_by equals usr.id
                                                     select new CommentsViewModel
                                                     {
                                                         tbl_application_id = c.tbl_application_id.GetValueOrDefault(),
                                                         //tbl_files_id = c.tbl_files_id,
                                                         //fileName = f.filename,
                                                         comment_to = c.comment_to,
                                                         comment = c.comment,
                                                         commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                         created_by = c.created_by,
                                                         modified_by = c.modified_by,
                                                         date_created = c.date_created,
                                                         date_modified = c.date_modified
                                                     }).Where(u => u.comment_to == "User To CENRO").OrderByDescending(d => d.date_created);

                //Set the value for announcementID (used to display the required documents depending on permit type)
                int announcementID = 0;
                var applicationInfo = _context.tbl_application.Where(a => a.id == applid).FirstOrDefault();
                switch (applicationInfo.tbl_permit_type_id)
                {

                    case 1: //1   Permit to Import
                        announcementID = 2; //2   Permit to Import Requirements
                        break;
                    case 2: //2   Permit to Purchase
                        announcementID = 3; //3   Permit to Purchase Requirements
                        break;
                    case 3: //3   Permit to Sell
                        announcementID = 4; //4   Permit to Sell Requirements
                        break;
                    case 4: //4   Transfer of Ownership
                        announcementID = 7; //7   Transfer of Ownership Requirements
                        break;
                    case 5: //5   Authority to Lease
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 6: //6   Authority to Rent
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 7: //7   Authority to Lend
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 13: //13  Certificate of Registration
                        announcementID = 5; //5   Certificate of Registration Requirements
                        break;
                    case 14: //14  Permit to Re - sell / Transfer Ownership
                        announcementID = 7; //7   Transfer of Ownership Requirements
                        break;
                }
                //Get list of required documents from tbl_announcement
                var requirements = _context.tbl_announcement.Where(a => a.id == announcementID).FirstOrDefault();
                ViewBag.RequiredDocsList = requirements.announcement_content;
                //End for required documents

                //Application ChaisawList
                //var applicationChainsaws = _context.tbl_chainsaw.Where(g => g.tbl_application_id == applicID).ToList();

                var applicationChainsaws = (from cs in _context.tbl_chainsaw
                                            join b in _context.tbl_brands on cs.brand_id equals b.id into brandGroup
                                            from brandTBL in brandGroup.DefaultIfEmpty()
                                            where cs.tbl_application_id == applicID
                                            select new tbl_chainsaw
                                            {
                                                Id = cs.Id,
                                                user_id = cs.user_id,
                                                tbl_application_id = cs.tbl_application_id,
                                                brand_id = brandTBL.id,
                                                Brand = brandTBL.name,
                                                Model = cs.Model,
                                                Engine = cs.Engine,
                                                Power = cs.Power,
                                                remarks = cs.remarks,
                                                status = cs.status,
                                                watt = cs.watt,
                                                hp = cs.hp,
                                                watt_dec = cs.watt_dec,
                                                hp_dec = cs.hp_dec,
                                                gb = cs.gb,
                                                supplier = cs.supplier,
                                                date_purchase = cs.date_purchase,
                                                is_active = cs.is_active,
                                                date_created = cs.date_created,
                                                date_modified = cs.date_modified,
                                                created_by = cs.created_by,
                                                modified_by = cs.modified_by,
                                                chainsaw_serial_number = cs.chainsaw_serial_number,
                                                chainsaw_date_of_registration = cs.chainsaw_date_of_registration,
                                                chainsaw_date_of_expiration = cs.chainsaw_date_of_expiration,
                                                specification = cs.specification,
                                                purpose = cs.purpose,
                                            }).ToList();

                mymodel.tbl_Chainsaws = applicationChainsaws;
                return View(mymodel);
            }
        }

        [HttpPost]
        public IActionResult EditApplication(ViewModel? viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usid = Convert.ToInt32(viewMod.applicantViewModels.tbl_user_id);
            int applid = Convert.ToInt32(viewMod.applicantViewModels.id);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();


            if (viewMod.applicantViewModels.id != null)
            {


                //string buttonClicked = SubmitButton;

                //updating the application
                var appliDB = _context.tbl_application.Where(m => m.id == viewMod.applicantViewModels.id).FirstOrDefault();

                //if application is new (not for renewal)
                if (appliDB.renew_from == null)
                {
                    //appliDB.id = applid;
                    //appliDB.qty = viewMod.applicantViewModels.qty; //Commented 2024-01-08
                    //appliDB.purpose = viewMod.applicantViewModels.purpose; //Commented 2024-01-08
                    //appliDB.expected_time_arrival = viewMod.applicantViewModels.expectedTimeArrived;
                    //appliDB.expected_time_release = viewMod.applicantViewModels.expectedTimeRelease;
                    appliDB.date_modified = DateTime.Now;
                    appliDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
                    //appliDB.supplier_address = viewMod.applicantViewModels.address; //Commented 2024-01-08
                    //appliDB.date_of_inspection = viewMod.applicantViewModels.inspectionDate; //Commented 2024-01-08
                    //appliDB.tbl_specification_id = viewMod.applicantViewModels.specification; //Commented 2024-01-08
                    //if (appliDB.tbl_permit_type_id == 2 || appliDB.tbl_permit_type_id == 3) //For Permit to Purchase and Permit to Sell
                    //{
                    //    appliDB.coordinatedWithEnforcementDivision = viewMod.applicantViewModels.coordinatedWithEnforcementDivision;
                    //}
                    _context.SaveChanges();

                    //Log User Activity
                    var referenceNo = appliDB.ReferenceNo;
                    LogUserActivity("EditApplication", "Application Changes", $"Changes saved on {referenceNo}", apkDateTime: DateTime.Now);

                    //if (appliDB.tbl_permit_type_id == 13)
                    //{
                    //    var csawDB = _context.tbl_chainsaw.Where(c => c.tbl_application_id == appliDB.id).FirstOrDefault();
                    //    csawDB.Brand = viewMod.applicantViewModels.chainsawBrand;
                    //    csawDB.Model = viewMod.applicantViewModels.chainsawModel;
                    //    csawDB.Engine = viewMod.applicantViewModels.Engine;
                    //    csawDB.Power = viewMod.applicantViewModels.powerSource;
                    //    if (viewMod.applicantViewModels.powerSource == "Gas")
                    //    {
                    //        csawDB.hp = viewMod.applicantViewModels.hp;
                    //        csawDB.watt = null;
                    //    }
                    //    else
                    //    {
                    //        csawDB.watt = viewMod.applicantViewModels.Watt;
                    //        csawDB.hp = null;
                    //    }
                    //    csawDB.gb = viewMod.applicantViewModels.gb;
                    //    csawDB.chainsaw_serial_number = viewMod.applicantViewModels.chainsaw_serial_number;
                    //    csawDB.supplier = viewMod.applicantViewModels.chainsawSupplier;
                    //    csawDB.date_purchase = viewMod.applicantViewModels.date_purchase;
                    //    _context.SaveChanges();
                    //}
                    //Saving a file
                    if (viewMod.filesUpload != null)
                    {
                        var folderName = usid + "_" + applid;
                        foreach (var file in viewMod.filesUpload.Files)
                        {
                            var filesDB = new tbl_files();
                            FileInfo fileInfo = new FileInfo(file.FileName);
                            string path = Path.Combine(WebHostEnvironment.ContentRootPath, "wwwroot/Files/" + folderName);

                            //create folder if not exist
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);


                            string fileNameWithPath = Path.Combine(path, file.FileName);

                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            filesDB.tbl_application_id = viewMod.applicantViewModels.id;
                            filesDB.created_by = viewMod.applicantViewModels.tbl_user_id;
                            filesDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
                            filesDB.date_created = DateTime.Now;
                            filesDB.date_modified = DateTime.Now;
                            filesDB.filename = file.FileName;
                            filesDB.path = path;
                            filesDB.tbl_file_type_id = fileInfo.Extension;
                            filesDB.tbl_file_sources_id = fileInfo.Extension;
                            filesDB.file_size = Convert.ToInt32(file.Length);
                            filesDB.version = 1;
                            _context.tbl_files.Add(filesDB);
                            _context.SaveChanges();

                            //Log User Activity
                            LogUserActivity("EditApplication", "File Upload", $"File(s) uploaded on {folderName} for {referenceNo}", apkDateTime: DateTime.Now);
                            //Matching of tbl_files to tbl_document_checklist
                            foreach (var item in viewMod.fileChecklistViewModel)
                            {
                                if (item.FileNames != null)
                                {
                                    foreach (var item2 in item.FileNames)
                                    {
                                        if (item2 == file.FileName)
                                        {
                                            var filesChecklistBridge = new tbl_files_checklist_bridge();

                                            filesChecklistBridge.tbl_document_checklist_id = item.tbl_document_checklist_id;
                                            filesChecklistBridge.tbl_files_id = filesDB.Id;
                                            filesChecklistBridge.status = "Pending";
                                            _context.tbl_files_checklist_bridge.Add(filesChecklistBridge);
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Email
                    var subject = "Permit Application Status";
                    var body = "Greetings! \n You have successfully updated your application.";
                    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);

                }

                else //this means that application is for renewal
                {
                    //Saving a file
                    if (viewMod.filesUpload != null)
                    {
                        var folderName = usid + "_" + applid;
                        foreach (var file in viewMod.filesUpload.Files)
                        {
                            var filesDB = new tbl_files();
                            FileInfo fileInfo = new FileInfo(file.FileName);
                            string path = Path.Combine(WebHostEnvironment.ContentRootPath, "wwwroot/Files/" + folderName);

                            //create folder if not exist
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);


                            string fileNameWithPath = Path.Combine(path, file.FileName);

                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            filesDB.tbl_application_id = viewMod.applicantViewModels.id;
                            filesDB.created_by = viewMod.applicantViewModels.tbl_user_id;
                            filesDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
                            filesDB.date_created = DateTime.Now;
                            filesDB.date_modified = DateTime.Now;
                            filesDB.filename = file.FileName;
                            filesDB.path = path;
                            filesDB.tbl_file_type_id = fileInfo.Extension;
                            filesDB.tbl_file_sources_id = fileInfo.Extension;
                            filesDB.file_size = Convert.ToInt32(file.Length);
                            filesDB.version = 1;
                            _context.tbl_files.Add(filesDB);
                            _context.SaveChanges();

                            //Log User Activity
                            var referenceNo = appliDB.ReferenceNo;
                            LogUserActivity("EditApplication", "File Upload", $"File(s) uploaded on {folderName} for {referenceNo}", apkDateTime: DateTime.Now);
                            //Matching of tbl_files to tbl_document_checklist
                            foreach (var item in viewMod.fileChecklistViewModel)
                            {
                                if (item.FileNames != null)
                                {
                                    foreach (var item2 in item.FileNames)
                                    {
                                        if (item2 == file.FileName)
                                        {
                                            var filesChecklistBridge = new tbl_files_checklist_bridge();

                                            filesChecklistBridge.tbl_document_checklist_id = item.tbl_document_checklist_id;
                                            filesChecklistBridge.tbl_files_id = filesDB.Id;
                                            filesChecklistBridge.status = "Pending";
                                            _context.tbl_files_checklist_bridge.Add(filesChecklistBridge);
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            //(for return view)
            //var applicationlist = from a in _context.tbl_application
            //                      where a.tbl_user_id == usid && a.id == applid
            //                      select a;
            ////CODE FOR FILE DOWNLOAD
            //int applicID = Convert.ToInt32(viewMod.applicantViewModels.id);
            ////File Paths from Database
            //var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID).ToList();
            //List<tbl_files> files = new List<tbl_files>();

            //foreach (var fileList in filesFromDB)
            //{
            //    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
            //    //files.Add(new tbl_files { filename = f });
            //}

            //viewMod.tbl_Files = files;
            ////END FOR FILE DOWNLOAD

            ////HISTORY
            //var applicationtypelist = _context.tbl_application_type;
            //var applicationMod = (from a in applicationlist
            //                      join usr in _context.tbl_user on a.tbl_user_id equals usr.id
            //                      //join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
            //                      join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
            //                      join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
            //                      join pS in _context.tbl_permit_status on a.status equals pS.id
            //                      where a.tbl_user_id == usid && a.id == applid
            //                      select new ApplicantListViewModel
            //                      {
            //                          id = a.id,
            //                          tbl_user_id = usid,
            //                          full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
            //                          email = usr.email,
            //                          permit_type = pT.name,
            //                          permit_status = pS.status,
            //                          //user_type = usrtyps.name,
            //                          comment = usr.comment,
            //                          qty = a.qty,
            //                          specification = a.tbl_specification_id,
            //                          inspectionDate = a.date_of_inspection,
            //                          address = a.supplier_address,
            //                          //expectedTimeArrived = a.expected_time_arrival,
            //                          //expectedTimeRelease = a.expected_time_release,
            //                          purpose = a.purpose
            //                      }).FirstOrDefault();

            ////Set the value for announcementID (used to display the required documents depending on permit type)
            //int announcementID = 0;
            //var applicationInfo = _context.tbl_application.Where(a => a.id == applid).FirstOrDefault();
            //switch (applicationInfo.tbl_permit_type_id)
            //{

            //    case 1: //1   Permit to Import
            //        announcementID = 2; //2   Permit to Import Requirements
            //        break;
            //    case 2: //2   Permit to Purchase
            //        announcementID = 3; //3   Permit to Purchase Requirements
            //        break;
            //    case 3: //3   Permit to Sell
            //        announcementID = 4; //4   Permit to Sell Requirements
            //        break;
            //    case 4: //4   Transfer of Ownership
            //        announcementID = 7; //7   Transfer of Ownership Requirements
            //        break;
            //    case 5: //5   Authority to Lease
            //        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
            //        break;
            //    case 6: //6   Authority to Rent
            //        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
            //        break;
            //    case 7: //7   Authority to Lend
            //        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
            //        break;
            //    case 13: //13  Certificate of Registration
            //        announcementID = 5; //5   Certificate of Registration Requirements
            //        break;
            //    case 14: //14  Permit to Re - sell / Transfer Ownership
            //        announcementID = 7; //7   Transfer of Ownership Requirements
            //        break;
            //}
            ////Get list of required documents from tbl_announcement
            //var requirements = _context.tbl_announcement.Where(a => a.id == announcementID).FirstOrDefault();
            //ViewBag.RequiredDocsList = requirements.announcement_content;
            ////End for required documents
            //ViewBag.Message = "Save Success";
            //viewMod.applicantViewModels = applicationMod;

            ////return View(viewMod);
            return RedirectToAction("EditApplication", "Application", new { uid = usid, appid = applid });
        }

        //For File Download
        public FileResult DownloadFile(string fileName, string path)
        {
            //Build the File Path.
            string pathWithFilename = path + "//" + fileName;
            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(pathWithFilename);
            //Log Download Initiated
            LogUserActivity("Download", "Download File", $"File download initiated. {fileName}", apkDateTime: DateTime.Now);
            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public IActionResult CommentSection(int? uid, int? appid, ViewModel model)
        {
            var commentsTbl = new tbl_comments();
            //commentsTbl.id = model.tbl_Comments.id;
            commentsTbl.tbl_application_id = Convert.ToInt32(appid);
            //commentsTbl.tbl_files_id = model.tbl_Comments.tbl_files_id;
            commentsTbl.comment_to = model.tbl_Comments.comment_to;
            commentsTbl.comment = model.tbl_Comments.comment;
            commentsTbl.created_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.modified_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.date_created = DateTime.Now;
            commentsTbl.date_modified = DateTime.Now;
            _context.tbl_comments.Add(commentsTbl);
            _context.SaveChanges();
            //return RedirectToAction("AccountsApproval?uid="+uid, "AccountManagement");
            //Url.Action("A","B",new{a="x"})

            //Log User Activity
            LogUserActivity("Comments", "New Comment", $"Added new comment on Application#{appid}", apkDateTime: DateTime.Now);
            return RedirectToAction("EditApplication", "Application", new { uid = uid, appid = appid });
        }

        [HttpPost]
        public IActionResult UploadProofOfPayment(ViewModel model, string actionName)
        {
            int applicationID = Convert.ToInt32(model.tbl_Application.id);
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int loggedUserRegionId = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("regionID").Value);
            //var action = ViewContext.RouteData.Values["action"].ToString();
            //var action = ViewBag.ActionName;


            var folderName = loggedUserID + "_" + applicationID;

            //var folder = @"wwwroot/Files/ChainsawImporterPermit/";

            //if (actionName == "ImportPermits")
            //{

            //    folder = @"wwwroot/Files/ChainsawImporterPermit/";
            //}


            var referenceNo = _context.tbl_application.Where(a => a.id == applicationID).Select(a => a.ReferenceNo).FirstOrDefault();
            var permitTypeId = _context.tbl_application.Where(a => a.id == applicationID).Select(a => a.tbl_permit_type_id).FirstOrDefault();
            //Saving a file
            if (model.filesUpload != null)
            {
                foreach (var file in model.filesUpload.Files)
                {
                    var filesDB = new tbl_files();
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string path = Path.Combine(WebHostEnvironment.ContentRootPath, "wwwroot/Files/" + folderName + "/ProofofPayment/");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string fileNameWithPath = Path.Combine(path, file.FileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    filesDB.tbl_application_id = applicationID;
                    filesDB.created_by = loggedUserID;
                    filesDB.modified_by = loggedUserID;
                    filesDB.date_created = DateTime.Now;
                    filesDB.date_modified = DateTime.Now;
                    filesDB.filename = file.FileName;
                    filesDB.path = path;
                    filesDB.tbl_file_type_id = fileInfo.Extension;
                    filesDB.tbl_file_sources_id = fileInfo.Extension;
                    filesDB.is_proof_of_payment = true;
                    filesDB.file_size = Convert.ToInt32(file.Length);
                    filesDB.version = 1;
                    _context.tbl_files.Add(filesDB);
                    _context.SaveChanges();
                }
                //Log User Activity
                LogUserActivity("Proof of Payment", "File Upload", $"File(s) uploaded on {folderName} for {referenceNo}", apkDateTime: DateTime.Now);
            }

            var applicationPayment = model.tbl_Application_Payment;
            applicationPayment.tbl_application_id = applicationID;
            applicationPayment.allow_edit = true;
            applicationPayment.date_created = DateTime.Now;
            applicationPayment.date_modified = DateTime.Now;
            _context.tbl_application_payment.Add(applicationPayment);
            _context.SaveChanges();

            //Log User Activity
            LogUserActivity("Proof of Payment", "Payment Details saved", $"Payment info saved for {referenceNo}", apkDateTime: DateTime.Now);
            //modify permit status
            int stats = 7; // 7 - Payment Verification (Inspector)
            var statsName = _context.tbl_permit_workflow_step.Where(pwfs => pwfs.permit_type_code == "1" && pwfs.workflow_step_code == stats.ToString()).Select(pwfs => pwfs.name).FirstOrDefault();
            //SAVE CHANGES TO DATABASE
            DateTime? dateDueOfficer = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
            var appli = new tbl_application() { id = applicationID, status = stats, date_modified = DateTime.Now, modified_by = loggedUserID, date_due_for_officers = dateDueOfficer };
            //var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
            using (_context)
            {
                _context.tbl_application.Attach(appli);
                _context.Entry(appli).Property(x => x.status).IsModified = true;
                _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                _context.Entry(appli).Property(x => x.date_due_for_officers).IsModified = true;
                _context.SaveChanges();

                var permitTypeName = _context.tbl_permit_type.Where(x => x.id == permitTypeId).Select(x => x.name).FirstOrDefault();
                var approvers = _workflowService.GetNextStepApprover(1, 1);

                foreach (var approver in approvers.Result)
                {

                    var notificationModel = ModelCreation.PermitNotificationForApproverModel(permitTypeName + " for approval",
                                                                                            "Please see the reference no: " + referenceNo,
                                                                                            approver,
                                                                                            loggedUserID,
                                                                                            loggedUserRegionId);
                    _notificationService.Insert(notificationModel, loggedUserID);
                }

                var applicantNotificationModel = ModelCreation.PermitNotificationForApplicantModel(
                                                                            "The status of your application is: " + statsName,
                                                                            "Please see the reference no: " + referenceNo,
                                                                            loggedUserID,
                                                                            loggedUserID);
                _notificationService.Insert(applicantNotificationModel, loggedUserID);
            }



            //return View();
            //return new EmptyResult();
            return RedirectToAction(actionName, "Application");
        }

        [HttpPost]
        public IActionResult EditPayment(int? uid, int? appid, ViewModel model)
        {
            //Editing of Payment
            var applicationPayment = _context.tbl_application_payment.Where(ap => ap.tbl_application_id == appid).FirstOrDefault();
            applicationPayment.Amount = model.tbl_Application_Payment.Amount;
            applicationPayment.OR_Number = model.tbl_Application_Payment.OR_Number;
            applicationPayment.Date_of_Payment = model.tbl_Application_Payment.Date_of_Payment;
            applicationPayment.date_modified = DateTime.Now;
            _context.SaveChanges();

            var referenceNo = _context.tbl_application.Where(a => a.id == appid).Select(a => a.ReferenceNo).FirstOrDefault();
            //Log User Activity
            LogUserActivity("Proof of Payment", "Payment Details updated", $"Payment info updated for {referenceNo}", apkDateTime: DateTime.Now);

            return RedirectToAction("EditApplication", "Application", new { uid = uid, appid = appid });
        }

        [HttpPost]
        public JsonResult RenewApplication(int oldApplicationID)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int loggedUserRegionId = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("regionID").Value);

            var oldApplication = _context.tbl_application.Where(a => a.id == oldApplicationID).FirstOrDefault();
            var renewApplication = new tbl_application();
            var isRenewForThisExist = _context.tbl_application.Any(a => a.renew_from == oldApplicationID);

            //Copy contents of previous application
            if (oldApplication != null && DateTime.Now.AddMonths(2) >= oldApplication.date_of_expiration && isRenewForThisExist == false)
            {
                renewApplication = oldApplication;
                renewApplication.id = null; //remove id here
                renewApplication.date_of_registration = null;
                renewApplication.date_of_expiration = null;
                renewApplication.date_of_inspection = null;
                renewApplication.initial_date_of_inspection = null;
                renewApplication.created_by = loggedUserID;
                renewApplication.modified_by = loggedUserID;
                renewApplication.date_created = DateTime.Now;
                renewApplication.date_modified = DateTime.Now;
                renewApplication.date_due_for_officers = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
                renewApplication.status = 1;
                renewApplication.renew_from = oldApplicationID;
                renewApplication.ReferenceNo = null;
                renewApplication.original_renew_from = renewApplication.original_renew_from == null ? oldApplicationID : renewApplication.original_renew_from;
                _context.tbl_application.Add(renewApplication);
                _context.SaveChanges();

                int? appID = renewApplication.id;
                //Generate Reference Number

                var referenceNo = string.Empty;
                var legend = string.Empty;

                if (renewApplication.tbl_permit_type_id.HasValue && appID.HasValue)
                {
                    var legendEntity = _context.tbl_reference_legend.Where(a => a.permit_type_id == renewApplication.tbl_permit_type_id.Value).FirstOrDefault();

                    legend = legendEntity.legend;
                    referenceNo = ReferenceNumberGenerator.GenerateTransactionReference(legend, appID.Value);
                }


                var application = _context.tbl_application.Where(a => a.id == appID.Value).First<tbl_application>();
                application.ReferenceNo = referenceNo;
                _context.SaveChanges();

                //copy contents of tbl_application group
                var isOldApplicationGroupExist = _context.tbl_application_group.Any(a => a.tbl_application_id == oldApplicationID);
                if (isOldApplicationGroupExist == true)
                {
                    var oldApplicationGroup = _context.tbl_application_group.Where(a => a.tbl_application_id == oldApplicationID).ToList();
                    List<tbl_application_group> newApplicationGroup = oldApplicationGroup;
                    for (int a = 0; a < newApplicationGroup.Count; a++)
                    {
                        newApplicationGroup[a].id = 0;
                        newApplicationGroup[a].tbl_application_id = (int)renewApplication.id;
                        newApplicationGroup[a].date_modified = DateTime.Now;

                        _context.tbl_application_group.Add(newApplicationGroup[a]);
                        _context.SaveChanges();
                    }
                }

                //Copy Contents of tbl_chainsaw
                var isOldChainsawExist = _context.tbl_chainsaw.Any(a => a.tbl_application_id == oldApplicationID);
                if (isOldChainsawExist == true)
                {
                    var oldTBLChainsaw = _context.tbl_chainsaw.Where(a => a.tbl_application_id == oldApplicationID).ToList();
                    List<tbl_chainsaw> newTBLChainsaw = oldTBLChainsaw;
                    for (int a = 0; a < newTBLChainsaw.Count; a++)
                    {
                        newTBLChainsaw[a].Id = 0;
                        newTBLChainsaw[a].tbl_application_id = (int)renewApplication.id;
                        newTBLChainsaw[a].date_modified = DateTime.Now;

                        _context.tbl_chainsaw.Add(newTBLChainsaw[a]);
                        _context.SaveChanges();
                    }
                }

                //Copy contents of files

                string folderName = loggedUserID + "_" + renewApplication.id;
                string path = Path.Combine(EnvironmentHosting.ContentRootPath, "wwwroot/Files/" + folderName);

                var oldApplicationFiles = _context.tbl_files.Where(f => f.tbl_application_id == oldApplicationID).ToList();
                var oldFilePath = oldApplicationFiles.Select(p => p.path).FirstOrDefault();

                string newPath = Path.Combine(EnvironmentHosting.ContentRootPath, "wwwroot/Files/" + folderName); ;
                CopyFiles.CopyFilesRecursively(oldFilePath, newPath);

                //var newApplicationFiles = oldApplicationFiles.ToList();
                var newApplicationFiles = oldApplicationFiles.Where(f => f.is_proof_of_payment != true).ToList(); //copy old application files to newApplicationFiles except proof of payment
                //newApplicationFiles.Select(n => n.path).ToList();

                //Copy contents from tbl_files based on previous application
                for (int i = 0; i < newApplicationFiles.Count; i++)
                {
                    int oldFileID = newApplicationFiles[i].Id;
                    newApplicationFiles[i].Id = 0;
                    newApplicationFiles[i].path = newApplicationFiles[i].path.Replace(loggedUserID + "_" + oldApplicationID, folderName);
                    newApplicationFiles[i].date_modified = DateTime.Now;
                    newApplicationFiles[i].tbl_application_id = renewApplication.id;

                    _context.tbl_files.Add(newApplicationFiles[i]);
                    _context.SaveChanges();

                    var isFilesChcklstBrdgeNotNull = _context.tbl_files_checklist_bridge.Where(f => f.tbl_files_id == oldFileID).Any();
                    if (isFilesChcklstBrdgeNotNull == true)
                    {
                        var filesChcklstBrdge = _context.tbl_files_checklist_bridge.Where(f => f.tbl_files_id == oldFileID).ToList();
                        //var newFilesChcklstBrdge = new tbl_files_checklist_bridge;

                        //Copy tagged checklist of files based from previous application
                        for (int j = 0; j < filesChcklstBrdge.Count; j++)
                        {
                            int oldBridgeID = filesChcklstBrdge[j].id;
                            filesChcklstBrdge[j].id = 0;
                            filesChcklstBrdge[j].tbl_files_id = newApplicationFiles[i].Id;
                            _context.tbl_files_checklist_bridge.Add(filesChcklstBrdge[j]);
                            _context.SaveChanges();

                            //renewApplication.id is the new application ID
                            //newApplicationFiles[i].Id is the new file ID
                            //filesChcklstBrdge[j].id is the new bridge ID

                            //Copy comments from previous application and replace bridge id and files id

                            //check if bridge id exist on tbl_comments
                            var isCommentExistOnBridgeID = _context.tbl_comments.Where(f => f.bridge_id == oldBridgeID).Any();
                            if (isCommentExistOnBridgeID == true)
                            {
                                var commentsTBL = _context.tbl_comments.Where(c => c.bridge_id == oldBridgeID).ToList();
                                for (int k = 0; k < commentsTBL.Count; k++)
                                {
                                    commentsTBL[k].id = 0;
                                    commentsTBL[k].tbl_application_id = renewApplication.id;
                                    commentsTBL[k].tbl_files_id = newApplicationFiles[i].Id;
                                    commentsTBL[k].bridge_id = filesChcklstBrdge[j].id;
                                    _context.tbl_comments.Add(commentsTBL[k]);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    }

                }

                //Log User Activity
                LogUserActivity("RenewApplication", "Application Renewal", $"Application has been renewed. {referenceNo}", apkDateTime: DateTime.Now);
                var permitTypeId = renewApplication.tbl_permit_type_id.Value;
                var permitTypeName = _context.tbl_permit_type.Where(x => x.id == permitTypeId).Select(x => x.name).FirstOrDefault();
                var approvers = _workflowService.GetNextStepApprover(1, 1);

                foreach (var approver in approvers.Result)
                {

                    var notificationModel = ModelCreation.PermitNotificationForApproverModel(permitTypeName + " for approval",
                                                                                            "Please see the reference no: " + referenceNo,
                                                                                            approver,
                                                                                            loggedUserID,
                                                                                            loggedUserRegionId);
                    _notificationService.Insert(notificationModel, loggedUserID);
                }

                //folder format loggedUserID_renewApplication.id
                return Json(renewApplication.id);
            }
            else
            {
                return Json(false);
            }

        }

        [HttpGet, ActionName("GetFileTags")]
        public JsonResult GetFileTags(int fileID)
        {
            try
            {
                //int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

                var fileTags = (from br in _context.tbl_files_checklist_bridge
                                join chck in _context.tbl_document_checklist on br.tbl_document_checklist_id equals chck.id
                                join f in _context.tbl_files on br.tbl_files_id equals f.Id
                                where br.tbl_files_id == fileID
                                select new
                                {
                                    fileName = f.filename,
                                    fileTag = chck.name
                                }).ToList();

                return Json(fileTags);
            }
            catch
            {
                // Log the exception or handle it as needed
                Console.Error.WriteLine($"An error occurred.");

                // You can return an error response or a default value as needed
                return Json(new { error = "An error occurred while processing the request." });
            }
        }

        [HttpPost]
        public IActionResult ReplaceDocument(IFormFile replacementFile, int fileID)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            if (replacementFile != null && replacementFile.Length > 0)
            {
                //GET CONTENTS FROM DATABASE
                var filesDB = _context.tbl_files.Where(f => f.Id == fileID).FirstOrDefault();
                var applicationFromDB = _context.tbl_application.Where(a => a.id == filesDB.tbl_application_id).FirstOrDefault();

                FileInfo fileInfo = new FileInfo(replacementFile.FileName);

                //Foler Name : userID_applicationID
                string folderName = applicationFromDB.tbl_user_id + "_" + filesDB.tbl_application_id;
                string fileName = filesDB.filename;//Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePhoto.FileName);

                //Remove the version identifer on filename
                if (filesDB.version != 1)
                {
                    // Find the index of 'v'
                    int indexOfdash = fileName.IndexOf('-');

                    // Check if 'v' is found and get the substring after it
                    if (indexOfdash != -1 && indexOfdash + 1 < fileName.Length)
                    {
                        fileName = fileName.Substring(indexOfdash + 1);
                    }
                }
                fileName = "v" + (Convert.ToInt32(filesDB.version) + 1) + "-" + fileName;

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/" + folderName);

                //Delete the old one before writing
                var oldPathWithFilename = Path.Combine(filesDB.path, filesDB.filename);
                if (System.IO.File.Exists(oldPathWithFilename))
                {
                    System.IO.File.Delete(oldPathWithFilename);
                }

                //create folder if not exist
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    replacementFile.CopyTo(stream);
                }

                //Changes for tbl_files
                filesDB.filename = fileName;
                filesDB.date_modified = DateTime.Now;
                filesDB.modified_by = loggedUserID;
                filesDB.version = Convert.ToInt32(filesDB.version) + 1; ;
                filesDB.file_size = Convert.ToInt32(replacementFile.Length);
                _context.SaveChanges();

                //Changes for tbl_files_checklist_bridge (set to pending)
                var fChBridge = _context.tbl_files_checklist_bridge.Where(b => b.tbl_files_id == fileID).ToList();
                for (int i = 0; i < fChBridge.Count; i++)
                {
                    fChBridge[i].status = "Pending";
                    _context.SaveChanges();
                    //fChBridge[i].id;
                    //Add new comment on tbl_comments
                    var commentsTbl = new tbl_comments();
                    commentsTbl.tbl_application_id = Convert.ToInt32(filesDB.tbl_application_id);
                    commentsTbl.tbl_files_id = fileID;
                    commentsTbl.comment = "Document was replaced at " + DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt") + " status was changed to pending for re-checking.";
                    commentsTbl.created_by = loggedUserID;
                    commentsTbl.modified_by = loggedUserID;
                    commentsTbl.date_created = DateTime.Now;
                    commentsTbl.date_modified = DateTime.Now;
                    commentsTbl.bridge_id = fChBridge[i].id;
                    _context.tbl_comments.Add(commentsTbl);
                    _context.SaveChanges();
                }

                //Log User Activity
                LogUserActivity("Application", "Replace rejected document", $"Rejected document has been replaced with {fileName}", apkDateTime: DateTime.Now);

                string success = "success";
                return Ok(new { success });
            }

            return BadRequest("Invalid file.");
        }
    }
}
