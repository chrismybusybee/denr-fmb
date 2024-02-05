using FMB_CIS.Data;
using FMB_CIS.Models;
using FMB_CIS.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class ReportGenerationController : Controller
    {


        private readonly LocalContext _context;

        public ReportGenerationController(LocalContext context) {
            _context = context;
        }


        [RequiresAccess(allowedAccessRights = "allow_page_report_generation")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DynamicReportGeneration()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            //var applicationlist = from a in _context.tbl_application
            //                      where a.tbl_user_id == userID
            //                      //where a.tbl_application_type_id == 3
            //                      select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in _context.tbl_application
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 join pSs in _context.tbl_permit_statuses on a.status equals pSs.id
                                 //where pT.name == "Permit to Import"
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
                                     permit_status = pSs.status,
                                     permit_status_id = pSs.id,
                                     tbl_user_id = (int)usr.id,
                                     status = (int)a.status,
                                     qty = a.qty,
                                     ReferenceNo = a.ReferenceNo
                                 };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }



        public CsvResult GetCompletedImportPermit()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 2
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "ImportPermit_Completed_Applications.csv");
        }


        public CsvResult GetPendingImportPermit()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 2
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "ImportPermit_Pending_Applications.csv");
        }




        public CsvResult GetCompletedPermitToSell()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 3
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                          && pT.name == "Permit to Sell"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToSell_Completed_Applications.csv");
        }

        public CsvResult GetPendingPermitToSell()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 3
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                            && pT.name == "Permit to Sell"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToSell_Pending_Applications.csv");
        }



        public CsvResult GetCompletedPermitToPurchase()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 3
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                          && pT.name == "Permit to Purchase"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToPurchase_Completed_Applications.csv");
        }

        public CsvResult GetPendingPermitToPurchase()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 3
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Permit to Purchase"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToPurchase_Pending_Applications.csv");
        }


        public CsvResult GetCompletedPermitToLease()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                           && pT.name == "Authority to Lease"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToLease_Completed_Applications.csv");
        }


        public CsvResult GetPendingPermitToLease()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Authority to Lease"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToLease_Pending_Applications.csv");
        }

        public CsvResult GetCompletedPermitToRent()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                           && pT.name == "Authority to Rent"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToRent_Completed_Applications.csv");
        }

        public CsvResult GetPendingPermitToRent()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Authority to Rent"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToRent_Pending_Applications.csv");
        }


        public CsvResult GetCompletedPermitToLend()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                           && pT.name == "Authority to Lend"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToLend_Completed_Applications.csv");
        }

        public CsvResult GetPendingPermitToLend()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Authority to Lend"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToLend_Pending_Applications.csv");
        }

        public CsvResult GetCompletedPermitToResellAndTransferOfOwnership()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                           && pT.name == "Permit to Re-sell/Transfer Ownership"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToResellAndTransferOfOwnership_Completed_Applications.csv");
        }


        public CsvResult GetPendingPermitToResellAndTransferOfOwnership()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Permit to Re-sell/Transfer Ownership"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "PermitToResellAndTransferOfOwnership_Pending_Applications.csv");
        }


        public CsvResult GetCompletedCertificateOfRegistration()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where wfs.name == "For Permit Issuance"
                                           && pT.name == "Certificate of Registration"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "CertificateOfRegistration_Completed_Applications.csv");
        }


        public CsvResult GetPendingCertificateOfRegistration()
        {
            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    var applicationlist = from a in _context.tbl_application
                                              //where a.tbl_user_id == userID
                                          where a.tbl_application_type_id == 1
                                          select a;

                    var pendingStatus = new List<string>
                {
                   "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    var applicationMod = (from a in applicationlist
                                          join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                          join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                          join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                          join pS in _context.tbl_permit_status on a.status equals pS.id
                                          //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                          //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                          join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                                          where usr.tbl_region_id == userRegion
                                          where pendingStatus.Contains(wfs.name)
                                           && pT.name == "Certificate of Registration"
                                          //where a.tbl_user_id == userID
                                          orderby a.id descending
                                          select new ReportModel
                                          {
                                              ReferenceNo = a.ReferenceNo,
                                              id = a.id,
                                              applicationDate = a.date_created,
                                              qty = a.qty,
                                              full_name = usr.user_classification == "Individual" ? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix : usr.company_name,
                                              email = usr.email,
                                              contact = usr.contact_no,
                                              address = usr.street_address,
                                              application_type = appt.name,
                                              permit_type = pT.name,
                                              permit_status = wfs.name,
                                              tbl_user_id = (int)usr.id,
                                              date_due_for_officers = a.date_due_for_officers,
                                              isRead = false,
                                              currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                              currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic                              
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "CertificateOfRegistration_Pending_Applications.csv");
        }

    }
}
