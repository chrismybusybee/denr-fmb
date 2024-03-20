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
                                              renewCount = _context.tbl_application.Where(x => x.original_renew_from == a.id).Count()
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
                                              renewCount = _context.tbl_application.Where(x => x.original_renew_from == a.id).Count()
                                          }).ToList();


                    await csv.WriteRecordsAsync<ReportModel>(applicationMod);
                },
                "CertificateOfRegistration_Pending_Applications.csv");
        }


        //public CsvResult GenerateReportSummary()
        public CsvResult GenerateReportSummary(string TotalImport, string TotalSell, string TotalPurchase, string TotalCoR, string TotalCoRRenewed, string TotalLease, string TotalReSell, string TotalRent, string TotalLend)
        {
            bool isTotalImportChecked = !string.IsNullOrEmpty(TotalImport) && TotalImport == "on";
            bool isTotalSellChecked = !string.IsNullOrEmpty(TotalSell) && TotalSell == "on";
            bool isTotalPurchaseChecked = !string.IsNullOrEmpty(TotalPurchase) && TotalPurchase == "on";
            bool isTotalCoRChecked = !string.IsNullOrEmpty(TotalCoR) && TotalCoR == "on";
            bool isTotalCoRRenewedChecked = !string.IsNullOrEmpty(TotalCoRRenewed) && TotalCoRRenewed == "on";
            bool isTotalLeaseChecked = !string.IsNullOrEmpty(TotalLease) && TotalLease == "on";
            bool isTotalReSellChecked = !string.IsNullOrEmpty(TotalReSell) && TotalReSell == "on";
            bool isTotalRentChecked = !string.IsNullOrEmpty(TotalRent) && TotalRent == "on";
            bool isTotalLendChecked = !string.IsNullOrEmpty(TotalLend) && TotalLend == "on";

            return new CsvResult(
                async csv =>
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var userRegion = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();

                    

                    var pendingStatus = new List<string>
                    {
                       "For Evaluation", "For Permit Approval", "For Physical Inspection", "For Payment"
                    };

                    var applicationtypelist = _context.tbl_application_type;
                    // your code to retrieve data
                    //var applicationMod = (from pT in _context.tbl_permit_type
                    //                      join a in _context.tbl_application on pT.id equals a.tbl_permit_type_id into gj
                    //                      join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code }
                    //                      from a in gj.DefaultIfEmpty()
                    //                      where (isTotalImportChecked && pT.id == 1) ||
                    //                            (isTotalSellChecked && pT.id == 3) ||
                    //                            (isTotalPurchaseChecked && pT.id == 2) ||
                    //                            (isTotalCoRChecked && pT.id == 13) ||
                    //                            (isTotalLeaseChecked && pT.id == 5) ||
                    //                            (isTotalReSellChecked && pT.id == 14) ||
                    //                            (isTotalRentChecked && pT.id == 6) ||
                    //                            (isTotalLendChecked && pT.id == 7)
                    //                      group a by pT.name into g
                    //                      orderby g.Count() descending
                    //                      select new ReportCountsModel
                    //                      {
                    //                          PermitType = "Total " + g.Key,
                    //                          Counts = g.Count(),
                    //                      }).ToList();


                    //var applicationMod = (from pT in _context.tbl_permit_type
                    //                      join a in _context.tbl_application on pT.id equals a.tbl_permit_type_id into gj
                    //                      from a in gj.DefaultIfEmpty()
                    //                      where (isTotalImportChecked && pT.id == 1) ||
                    //                            (isTotalSellChecked && pT.id == 3) ||
                    //                            (isTotalPurchaseChecked && pT.id == 2) ||
                    //                            (isTotalCoRChecked && pT.id == 13) ||
                    //                            (isTotalLeaseChecked && pT.id == 5) ||
                    //                            (isTotalReSellChecked && pT.id == 14) ||
                    //                            (isTotalRentChecked && pT.id == 6) ||
                    //                            (isTotalLendChecked && pT.id == 7)
                    //                      group a by pT.name into g
                    //                      orderby g.Count() descending
                    //                      select new ReportCountsModel
                    //                      {
                    //                          PermitType = "Total " + g.Key,
                    //                          Counts = g.Count(),
                    //                      }).ToList();

                    var applicationMod = (from pT in _context.tbl_permit_type
                                          join a in _context.tbl_application on pT.id equals a.tbl_permit_type_id into gj
                                          from a in gj.DefaultIfEmpty()
                                          where (isTotalImportChecked && pT.id == 1) ||
                                                (isTotalSellChecked && pT.id == 3) ||
                                                (isTotalPurchaseChecked && pT.id == 2) ||
                                                (isTotalCoRChecked && pT.id == 13) ||
                                                (isTotalLeaseChecked && pT.id == 5) ||
                                                (isTotalReSellChecked && pT.id == 14) ||
                                                (isTotalRentChecked && pT.id == 6) ||
                                                (isTotalLendChecked && pT.id == 7)
                                          join s in _context.tbl_permit_workflow_step on a.status.ToString() equals s.workflow_code into sj
                                          from s in sj.DefaultIfEmpty()
                                          group new { a, s } by pT.name into g
                                          orderby g.Count() descending
                                          select new ReportCountsModel
                                          {
                                              PermitType = "Total " + g.Key,
                                              Counts = g.Count(),
                                              Pending = g.Count(x => x.a.status != 11),
                                              Completed = g.Count(x => x.a.status == 11)
                                          }).ToList();




                    // Add a separate row for "Total Certificates of Registration (Renewed)" if applicable
                    if (isTotalCoRRenewedChecked)
                    {
                        var renewed = _context.tbl_application.Where(a => a.tbl_permit_type_id == 13 && a.renew_from != null);
                        applicationMod.Add(new ReportCountsModel 
                        { 
                            PermitType = "Total Certificates of Registration (Renewed)",
                            Counts = renewed.Count(),
                            Pending = renewed.Count(x => x.status != 11),
                            Completed = renewed.Count(x => x.status == 11)
                        });
                    }

                    await csv.WriteRecordsAsync<ReportCountsModel>(applicationMod);
                },
                "ReportSummary.csv");
        }


    }
}
