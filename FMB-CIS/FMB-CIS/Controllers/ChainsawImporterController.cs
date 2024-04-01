
﻿using System;
using System.Net;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNet.Identity;
using System.Collections;
using Mapster;
using Microsoft.Extensions.Hosting;
using Services.Utilities;
using MailKit;
using System.Text.Json;
using Newtonsoft.Json;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
using Azure;

namespace FMB_CIS.Controllers
{
    //[Authorize(Roles = "Chainsaw Importer")]
    //[Authorize(Roles = "Chainsaw Importer and Seller")]
    //[Authorize(Roles = "Chainsaw Importer and Owner")]
    //[Authorize(Roles = "Chainsaw Importer, Owner and Seller")]
    [Authorize]
    public class ChainsawImporterController : Controller
    {

        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
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

        public ChainsawImporterController(IConfiguration configuration, LocalContext context, IEmailSender emailSender, IWebHostEnvironment _environment)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            EnvironmentHosting = _environment;
        }

        [RequiresAccess(allowedAccessRights = "allow_page_create_permit_to_import")]
        public IActionResult Index()
        {
            //Set Roles who can access this page
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            bool? usrStatus = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault();
            ViewModel model = new ViewModel();

            // Get Brands for DropdownList
            var brandList = _context.tbl_brands.Where(c=> c.is_active == true).ToList();
            ViewBag.brandList = brandList;

            model.tbl_Application_Group = new List<tbl_application_group>();
            //Get list of required documents from tbl_announcement
            var requirements = _context.tbl_announcement.Where(a => a.id == 2).FirstOrDefault(); // id = 2 for Permit to Import Requirements
            ViewBag.RequiredDocsList = requirements.announcement_content;
            //End for required documents

            //Document Checklist
            var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == 1 && c.is_active == true).ToList();
            model.tbl_Document_Checklist = myChecklist;

            //End for Document Checklist
            if (usrStatus != true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

                return View(model);
            // if (usrRoleID == 1 || usrRoleID == 4 || usrRoleID == 5 || usrRoleID == 7)
            // {
            //     return View(model);
            // }
            // else if (usrRoleID == 8 || usrRoleID == 9 || usrRoleID == 10 || usrRoleID == 11 || usrRoleID == 17) //(((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("DENR") == true)
            // {

            //     return RedirectToAction("ChainsawImporterApplicantsList", "ChainsawImporter");

            // }
            // else
            // {
            //     return RedirectToAction("Index", "Dashboard");
            // }
            //return View();
        }

        //public IActionResult ChainsawImporterApproval()
        //{
        //    return View();
        //}

        // POST: PermitToImportApplicationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ViewModel model)
        {
            List<tbl_application_group> myDeserializedObjList = (List<tbl_application_group>)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Dataxxx, typeof(List<tbl_application_group>));



            //try
            //{
            if (ModelState.IsValid)
                {
                    int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var usrDB = _context.tbl_user.Where(u => u.id == userID).FirstOrDefault();
                    //Get email and subject from templates in DB
                    var emailTemplates = _context.tbl_email_template.ToList();
                    var brandList = _context.tbl_brands.ToList();
                //DAL dal = new DAL();

                //SAVE permit application
                    model.tbl_Application = new tbl_application();
                    model.tbl_Application.tbl_application_type_id = 2;
                    model.tbl_Application.status = 1;
                    model.tbl_Application.tbl_user_id = userID;
                    model.tbl_Application.tbl_permit_type_id = 1;
                    model.tbl_Application.is_active = true;
                    model.tbl_Application.created_by = userID;
                    model.tbl_Application.modified_by = userID;
                    model.tbl_Application.date_created = DateTime.Now;
                    model.tbl_Application.date_modified = DateTime.Now;
                    model.tbl_Application.date_due_for_officers = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
                    _context.tbl_application.Add(model.tbl_Application);
                    _context.SaveChanges();
                    int? appID = model.tbl_Application.id;

                //Generate Reference Number

                    var referenceNo = string.Empty;
                    var legend = string.Empty;

                if (model.tbl_Application.tbl_permit_type_id.HasValue && appID.HasValue) {
                    var legendEntity = _context.tbl_reference_legend.Where(a => a.permit_type_id == model.tbl_Application.tbl_permit_type_id.Value).FirstOrDefault();

                    legend = legendEntity.legend;
                    referenceNo = ReferenceNumberGenerator.GenerateTransactionReference(legend, appID.Value);
                }
                    

                    var application = _context.tbl_application.Where(a => a.id == appID.Value).First<tbl_application>();
                    application.ReferenceNo = referenceNo;
                    _context.SaveChanges();

                //Application Grouping

                foreach (tbl_application_group childApplication in myDeserializedObjList)
                {
                    var brand = brandList.FirstOrDefault(a => a.id == childApplication.brand_id);

                    childApplication.id = 0;
                    childApplication.tbl_application_id = appID;
                    childApplication.created_by = userID;
                    childApplication.modified_by = userID;
                    childApplication.date_created = DateTime.Now;
                    childApplication.date_modified = DateTime.Now;
                    childApplication.brand = brand?.name;
                    _context.tbl_application_group.Add(childApplication);
                }

                _context.SaveChanges();

                //File Upload
                if (model.filesUpload != null)
                {
                    var folderName = userID + "_" + model.tbl_Application.id;
                    foreach (var file in model.filesUpload.Files)
                    {
                        try
                        {
                            var filesDB = new tbl_files();
                            FileInfo fileInfo = new FileInfo(file.FileName);
                            string path = Path.Combine(EnvironmentHosting.ContentRootPath, "wwwroot/Files/" + folderName);

                            //create folder if not exist
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);


                            string fileNameWithPath = Path.Combine(path, file.FileName);

                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            filesDB.tbl_application_id = appID;
                            filesDB.created_by = userID;
                            filesDB.modified_by = userID;
                            filesDB.date_created = DateTime.Now;
                            filesDB.date_modified = DateTime.Now;
                            filesDB.filename = file.FileName;
                            filesDB.path = path;
                            filesDB.tbl_file_type_id = fileInfo.Extension;
                            filesDB.tbl_file_sources_id = fileInfo.Extension;
                            filesDB.file_size = Convert.ToInt32(file.Length);
                            filesDB.status = "Pending";
                            filesDB.version = 1;
                            _context.tbl_files.Add(filesDB);
                            _context.SaveChanges();

                            //Matching of tbl_files to tbl_document_checklist
                            foreach (var item in model.fileChecklistViewModel)
                            {
                                foreach(var item2 in item.FileNames)
                                {
                                    if (item2 == file.FileName)
                                    {
                                        var filesChecklistBridge = new tbl_files_checklist_bridge();

                                        filesChecklistBridge.tbl_document_checklist_id = item.tbl_document_checklist_id;
                                        filesChecklistBridge.tbl_files_id = filesDB.Id;
                                        _context.tbl_files_checklist_bridge.Add(filesChecklistBridge);
                                        _context.SaveChanges();
                                    }
                                }                                
                            }
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("ArgumentNullException: " + e.Message);
                        }
                        catch (WebException e)
                        {
                            Console.WriteLine("WebException: " + e.Message);
                        }
                        catch (Exception e)

                        {

                            Console.WriteLine("Exception: " + e.Message);

                        }


                    }
                }
                
                //Email
                
                var emailTemplate = emailTemplates.Where(e => e.id == 14).FirstOrDefault();
                var subject = emailTemplate.email_subject;
                var BODY = emailTemplate.email_content.Replace("{FirstName}", usrDB.first_name);
                BODY = BODY.Replace("{ReferenceNo}", referenceNo);
                var body = BODY.Replace(Environment.NewLine, "<br/>");

                EmailSender.SendEmailAsync(((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value, subject, body);
                ModelState.Clear();
                ViewBag.Message = "Save Success";
                //Get list of required documents from tbl_announcement
                var requirements = _context.tbl_announcement.Where(a => a.id == 2).FirstOrDefault(); // id = 2 for Permit to Import Requirements
                ViewBag.RequiredDocsList = requirements.announcement_content;
                //End for required documents

                //Document Checklist
                var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == 1 && c.is_active == true).ToList();
                model.tbl_Document_Checklist = myChecklist;
                //End for Document Checklist

                //return View(model);

                //Log User Activity
                var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                string permitName = _context.tbl_permit_type.Where(p => p.id == model.tbl_Application.tbl_permit_type_id).Select(p => p.name).FirstOrDefault();
                LogUserActivity("Importer", "Submitted Application", $"{permitName} Application Submitted by {userEmail}. {referenceNo}", apkDateTime: DateTime.Now);

                return RedirectToAction("ImportPermits", "Application");
                
            }
                return View(model);
            //}
            //catch
            //{
            //    return View(model);
            //    //return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_approve_reject_applications")]
        [HttpGet]
        //[Url("?email={email}&code={code}")]
        public IActionResult ChainsawImporterApproval(string uid, string appid)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string Role = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
            //if (Role != "DENR CENRO" && Role != "DENR Implementing PENRO" && Role != "DENR Inspector" && Role != "DENR Regional Executive Director (RED)")
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
            ViewModel mymodel = new ViewModel();
            int applicID = Convert.ToInt32(appid);

            mymodel.uid = uid;
            mymodel.appid = applicID.ToString();

            //tbl_user user = _context.tbl_user.Find(uid);


            Workflow workflowModel = new Workflow();
            //Get the list of users
            var workflow = _context.tbl_permit_workflow.FirstOrDefault(e => e.workflow_code == "PERMIT_TO_IMPORT_CUSTOM");
            workflowModel = workflow.Adapt<Workflow>();

            //Get the list of steps
            var stepEntity = _context.tbl_permit_workflow_step.Where(o => o.workflow_code == workflowModel.workflow_code).ToList();
            workflowModel.steps = stepEntity.Adapt<List<WorkflowStep>>();

            //Get the list of nextsteps
            foreach (WorkflowStep workflowStep in workflowModel.steps)
            {
                var nextstepEntity = _context.tbl_permit_workflow_next_step.Where(o => o.workflow_code == workflowModel.workflow_code && o.workflow_step_code == workflowStep.workflow_step_code).ToList();
                //o.workflow_code == model.workflow_code &&
                workflowStep.nextSteps = nextstepEntity.Adapt<List<WorkflowNextStep>>();
            }

            mymodel.workflow = workflowModel;

            //CODE FOR FILE DOWNLOAD
           // int applicID = Convert.ToInt32(appid);
            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;
            ViewBag.uid = uid;
            ViewBag.appid = appid;

            //File Paths from Database
            //Files Uploaded by Applicant
            var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && f.is_proof_of_payment != true).ToList();
            List<tbl_files> files = new List<tbl_files>();

            foreach (var fileList in filesFromDB)
            {
                files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
            }

            mymodel.tbl_Files = files;
            
            //FILES UPLOADED BY INSPECTOR
            var filesFromInspector = (from f in _context.tbl_files
                                      join usr in _context.tbl_user on f.created_by equals usr.id
                                      where f.tbl_application_id == applicID && usr.tbl_user_types_id == 11 //11 is inspector role
                                      select f).ToList();
            List<tbl_files> inspectorFiles = new List<tbl_files>();

            foreach (var fileList in filesFromInspector)
            {
                inspectorFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
            }

            mymodel.filesUploadedByInspector = inspectorFiles;

            //FILES UPLOADED BY CENRO
            var filesFromCENRO = (from f in _context.tbl_files
                                  join usr in _context.tbl_user on f.created_by equals usr.id
                                  where f.tbl_application_id == applicID && (usr.tbl_user_types_id == 8 || usr.tbl_user_types_id == 9 || usr.tbl_user_types_id == 17)
                                  select f).ToList();
            List<tbl_files> cenroFiles = new List<tbl_files>();

            foreach (var fileList in filesFromCENRO)
            {
                cenroFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
            }

            mymodel.filesUploadedByCENRO = cenroFiles;

            //FILES FOR PROOF OF PAYMENT
            var filesFromPayment = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && f.is_proof_of_payment == true).ToList();
            List<tbl_files> paymentFiles = new List<tbl_files>();

            foreach (var fileList in filesFromPayment)
            {
                paymentFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
            }

            mymodel.proofOfPaymentFiles = paymentFiles;

            //END FOR FILE DOWNLOAD

            //Document Tagging and Checklist
            //var fileWithCommentsforDocTagging = (from f in _context.tbl_files
            //                                         //join c in _context.tbl_comments on f.Id equals c.tbl_files_id
            //                                         //join usr in _context.tbl_user on c.created_by equals usr.id
            //                                     where f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid)
            //                                     select new FilesWithComments
            //                                     {
            //                                         tbl_files_id = f.Id,
            //                                         filename = f.filename,
            //                                         tbl_application_id = f.tbl_application_id,
            //                                         tbl_files_status = f.status,
            //                                         //comment = c.comment
            //                                     }).ToList();

            //Get uploaded files and requirements
            //var fileWithCommentsforDocTagging = (from dc in _context.tbl_document_checklist
            //                                     join f in _context.tbl_files on dc.id equals f.checklist_id
            //                                     //join c in _context.tbl_comments on f.Id equals c.tbl_files_id
            //                                     //join usr in _context.tbl_user on c.created_by equals usr.id
            //                                     where dc.permit_type_id == 1 && f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && dc.is_active == true
            //                                     select new FilesWithComments
            //                                     {
            //                                         tbl_document_checklist_id = dc.id,
            //                                         tbl_document_checklist_name = dc.name,
            //                                         tbl_files_id = f.Id,
            //                                         filename = f.filename,
            //                                         tbl_application_id = f.tbl_application_id,
            //                                         tbl_files_status = f.status
            //                                         //comment = c.comment
            //                                     }).ToList();

            var fileWithCommentsforDocTagging = (from br in _context.tbl_files_checklist_bridge
                                                 join dc in _context.tbl_document_checklist on br.tbl_document_checklist_id equals dc.id
                                                 join f in _context.tbl_files on br.tbl_files_id equals f.Id
                                                 //join c in _context.tbl_comments on f.Id equals c.tbl_files_id
                                                 //join usr in _context.tbl_user on c.created_by equals usr.id
                                                 where dc.permit_type_id == 1 && f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && dc.is_active == true
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
                                                 }).OrderBy(o=>o.filename).ToList();

            var requiredDocumentList = _context.tbl_document_checklist.Where(c=>c.permit_type_id==1 && c.is_active==true).ToList();
            foreach(var reqList in requiredDocumentList)
            {
                //paymentFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                bool isReqAvailable = fileWithCommentsforDocTagging.Any(r => r.tbl_document_checklist_id == reqList.id);
                if(isReqAvailable == false)
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
                if (latestComment != null)
                {
                    fileWithCommentsforDocTagging[i].comment = latestComment.comment;
                    fileWithCommentsforDocTagging[i].tbl_comments_created_by = latestComment.created_by;
                }
            }

            mymodel.filesWithComments = fileWithCommentsforDocTagging;
            //End for Document Tagging and Checklist

            //Document Checklist
            //var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == 1).ToList();
            //mymodel.tbl_Document_Checklist = myChecklist;
            //var checkListApproval = (from f in _context.tbl_files
            //                         join req in _context.tbl_document_checklist on f.checklist_id equals req.id
            //                         where f.tbl_application_id == applicID && req.is_active == true
            //                         select new FileChecklistViewModel
            //                         {
            //                             FileName = f.filename,
            //                             tbl_document_checklist_name = req.name
            //                         }).ToList();           
            //mymodel.fileChecklistViewModel = checkListApproval;
            //End for Document Checklist

            if (uid == null || appid == null)
            {
                ModelState.AddModelError("", "Invalid Importer Application");
                return RedirectToAction("ChainsawImporterApplicantsList", "ChainsawImporter");
            }

            else
            {

                int usid = Convert.ToInt32(uid);
                int applid = Convert.ToInt32(appid);
                //var UserList = _context.tbl_user.ToList();
                //var UserInfo = UserList.Where(m => m.id == usid).ToList();

                //ViewModel mymodel = new ViewModel();
                var applicationlist = from a in _context.tbl_application
                                      where a.tbl_user_id == usid && a.id == applid
                                      select a;

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;

                var applicationMod = (from a in applicationlist
                                      join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                      //join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                      join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                      join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                      join pS in _context.tbl_permit_status on a.status equals pS.id
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
                                          company_name = usr.company_name,
                                          first_name = usr.first_name,
                                          middle_name = usr.middle_name,
                                          last_name = usr.last_name,
                                          suffix = usr.suffix,
                                          email = usr.email,
                                          contact = usr.contact_no,
                                          full_address = usr.street_address + " " + brngy.name + " " + ct.name + " " + prov.name + " " + reg.name,
                                          qty = a.qty,
                                          application_type = appt.name,
                                          permit_type = pT.name,
                                          permit_status = pS.status,
                                          status = Convert.ToInt32(a.status),
                                          permit_statuses = wfs.name,
                                          //user_type = usrtyps.name,
                                          valid_id = usr.valid_id,
                                          valid_id_no = usr.valid_id_no,
                                          birth_date = usr.birth_date.ToString(),
                                          region = reg.name,
                                          province = prov.name,
                                          city = ct.name,
                                          brgy = brngy.name,
                                          comment = usr.comment,
                                          initial_date_of_inspection = a.initial_date_of_inspection,
                                          inspectionDate = a.date_of_inspection,
                                          specification = a.tbl_specification_id,
                                          purpose = a.purpose,
                                          //expectedTimeArrived = a.expected_time_arrival,
                                          //expectedTimeRelease = a.expected_time_release,
                                          date_of_registration = a.date_of_registration,
                                          date_of_expiration = a.date_of_expiration,
                                          tbl_region_id = usr.tbl_region_id,
                                          renew_from = a.renew_from,
                                          currentStepCount = (int)Math.Ceiling((decimal)a.status / 2), // Soon be dynamic
                                          currentMaxCount = usr.tbl_region_id == 13 ? 6 : 10,// Soon be dynamic  
                                          ReferenceNo = a.ReferenceNo
                                      }).FirstOrDefault();
                applicationMod.currentPercentage = (applicationMod.currentStepCount * 100 / applicationMod.currentMaxCount);
                mymodel.applicantViewModels = applicationMod;

                //Check if this application has applied for renewal
                var checkRenewal = _context.tbl_application.Where(a=>a.renew_from == applid).FirstOrDefault();
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
                string commentType = "";
                if(((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Inspector"))
                {
                    commentType = "User To Inspector";
                }
                else if(((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("CENRO"))
                {
                    commentType = "User To CENRO";
                }
                //Display List of Comments for Application Approval (comments can be seen by user)
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
                                                  }).Where(u => u.comment_to == commentType).OrderByDescending(d => d.date_created);

                //Display List of Comments for Application Approval (comments for CENRO and Inspector)
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
                                                  }).Where(u => u.comment_to == "Inspector to CENRO").OrderByDescending(d => d.date_created);

                //To dispalay requirements on approval page
                var permitTypeOfThisApplication = _context.tbl_application.Where(a => a.id == applid).Select(a => a.tbl_permit_type_id).FirstOrDefault();
                int announcementID = 0;
                if (permitTypeOfThisApplication == 1)
                {
                    //permitTypeOfThisApplication (1 - Permit to Import)
                    announcementID = 2; // Announcement ID 2 - Permit to Import Requirements
                }

                if (announcementID != 0)
                {
                    //Get list of required documents from tbl_announcement
                    var requirements = _context.tbl_announcement.Where(a => a.id == announcementID).FirstOrDefault();
                    ViewBag.RequiredDocsList = requirements.announcement_content;
                    //End for required documents
                }


                //Application Grouping
                //var applicationGroups = _context.tbl_application_group.Where(g => g.tbl_application_id == applicID).ToList();
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

                //Proof of Payment
                var paymentDetails = _context.tbl_application_payment.Where(p => p.tbl_application_id == applid).FirstOrDefault();
                mymodel.tbl_Application_Payment = paymentDetails;

                return View(mymodel);
            }

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
        //[Url("?email={email}&code={code}")]
        public IActionResult ChainsawImporterApproval(ViewModel viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string Role = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
            int RoleID = _context.tbl_user_types.Where(ut => ut.name == Role).Select(ut => ut.id).FirstOrDefault();

            int? id = Convert.ToInt32(viewMod.appid);
            int? tbl_user_id = Convert.ToInt32(viewMod.uid);

            if (id == null)
            {
                return View();
            }
            else
            {
                int usid = Convert.ToInt32(tbl_user_id);
                int applid = Convert.ToInt32(id);
                int stats = 0;
                int emailTemplateID = 0;
                var emailTemplates = _context.tbl_email_template.ToList();
                string buttonClicked = viewMod.decision;
                bool registrationDateToBeChanged = false;
                bool expirationDateToBeChanged = false;
                DateTime? dateRegistration = null;
                DateTime? dateExpiration = null;
                DateTime? dateDueOfficer = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
                DateTime? dateInspectionInitial = null;
                bool initialInspectDateToBeChanged = false;
                DateTime? dateInspection = null;
                bool inspectDateToBeChanged = false;

                if (Role == "DENR Inspector" && viewMod.applicantViewModels.status == 1)
                {
                    dateInspectionInitial = Convert.ToDateTime(viewMod.applicantViewModels.initial_date_of_inspection);
                    initialInspectDateToBeChanged = true;
                }

                if (Role == "DENR Inspector" && (viewMod.applicantViewModels.status == 7 || viewMod.applicantViewModels.status == 8))
                {
                    dateInspection = Convert.ToDateTime(viewMod.applicantViewModels.inspectionDate);
                    inspectDateToBeChanged = true;
                }
                
                //File Upload
                if (viewMod.filesUpload != null)
                {
                    var folderName = tbl_user_id + "_" + id;

                    var folder = @"/INSPECTOR UPLOADS/";


                    if (Role == "DENR CENRO")
                    {
                        folder = @"/CENRO UPLOADS/";
                    }
                    string path = Path.Combine(EnvironmentHosting.ContentRootPath, "wwwroot/Files/" + folderName + folder);

                    foreach (var file in viewMod.filesUpload.Files)
                    {
                        var filesDB = new tbl_files();
                        FileInfo fileInfo = new FileInfo(file.FileName);

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);


                        string fileNameWithPath = Path.Combine(path, file.FileName);

                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        filesDB.tbl_application_id = id;
                        filesDB.created_by = loggedUserID;
                        filesDB.modified_by = loggedUserID;
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
                    }

                    //Log User Activity
                    LogUserActivity("ImporterApproval", "File Upload", $"The file(s) were uploaded to wwwroot/Files/{folderName}{folder}", apkDateTime: DateTime.Now);
                }

                if (buttonClicked == "Approve")
                {
                    if (viewMod.applicantViewModels.status <= 6) // Approval Process before payment
                    {
                        if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                        {
                            stats = 6; //For Payment which also means that it is already approved by CENRO
                            emailTemplateID = 38;
                            // email template id = 38 - Proceed to Payment
                            dateDueOfficer = null; //Next step is not assigned to the officer
                        }
                        else //Inspector
                        {
                            stats = 3; // Approved (Inspector) - For Cenro Approval                                                       
                        }
                    }
                    else //Final Approval of Application including Payments
                    {
                        if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                        {
                            stats = 11; //Payment and Application Approved (Inspector and CENRO)
                            emailTemplateID = 39; //Permit to Import (Approval)
                            registrationDateToBeChanged = true;
                            dateRegistration = DateTime.Now; //Permit will be considered registered once it has been approved
                            expirationDateToBeChanged = true;
                            dateExpiration = DateTime.Now.AddYears(3); //Permit to Expire after 3 years
                            dateDueOfficer = null; //Since task is done, no more due date for officer
                        }
                        else //Inspector
                        {
                            stats = 9; // Payment Verification (CENRO) - Approved by Inspector and to be verified by CENRO
                        }
                    }
                    //SAVE CHANGES TO DATABASE
                    //var applicationToUpdate = _context.tbl_application.Find(appID);
                    var appli = new tbl_application() { id = applid, status = stats, date_modified = DateTime.Now, modified_by = loggedUserID, initial_date_of_inspection = dateInspectionInitial, date_of_inspection = dateInspection, date_of_registration = dateRegistration, date_of_expiration = dateExpiration, date_due_for_officers = dateDueOfficer };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        _context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(appli).Property(x => x.initial_date_of_inspection).IsModified = initialInspectDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_inspection).IsModified = inspectDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_registration).IsModified = registrationDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_expiration).IsModified = expirationDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_due_for_officers).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    if (emailTemplateID != 0) //If emailTemplateID is 0, no email should be sent.
                    {
                        var emailTemplate = emailTemplates.Where(e => e.id == emailTemplateID).FirstOrDefault();

                        var subject = emailTemplate.email_subject;
                        var BODY = emailTemplate.email_content.Replace("{FirstName}", viewMod.applicantViewModels.first_name);
                        var body = BODY.Replace(Environment.NewLine, "<br/>");

                        EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                    }
                }
                else if (buttonClicked == "Decline")
                {
                    if (viewMod.applicantViewModels.status <= 6) // Rejection Process before payment
                    {
                        if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                        {
                            stats = 5; // 5 - Rejected(CENRO)
                            emailTemplateID = 16;
                            // email template id = 16 - Permit to Import (Rejection)

                        }
                        else
                        {
                            stats = 2; // 2 - Rejected(Inspector)
                            emailTemplateID = 16;
                            // email template id = 16 - Permit to Import (Rejection)
                        }
                    }
                    else
                    {
                        if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                        {
                            stats = 10; // 10 - Payment Rejected (CENRO)
                            emailTemplateID = 16;
                            // email template id = 16 - Permit to Import (Rejection)
                        }
                        else
                        {
                            stats = 8; // 8 - Payment Rejected (Inspector)
                            emailTemplateID = 16;
                            // email template id = 16 - Permit to Import (Rejection)
                        }
                    }

                    var appli = new tbl_application() { id = applid, status = stats, date_modified = DateTime.Now, modified_by = loggedUserID, date_due_for_officers = null };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        _context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_due_for_officers).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    if (emailTemplateID != 0) //If emailTemplateID is 0, no email should be sent.
                    {
                        var emailTemplate = emailTemplates.Where(e => e.id == emailTemplateID).FirstOrDefault();

                        var subject = emailTemplate.email_subject;
                        var BODY = emailTemplate.email_content.Replace("{FirstName}", viewMod.applicantViewModels.first_name);
                        var body = BODY.Replace(Environment.NewLine, "<br/>");

                        EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                    }
                }
                else
                {
                    stats = (int)(PermitStatusEnum)Enum.Parse(typeof(PermitStatusEnum), viewMod.next_step_code);

                    switch (stats)
                    {
                        case (int)PermitStatusEnum.APPLICATION_SUBMISSION:

                            break;
                        case (int)PermitStatusEnum.APPLICATION_ACCEPTANCE_REJECT:
                            emailTemplateID = 16;
                            break;
                        case (int)PermitStatusEnum.FOR_PHYSICAL_INSPECTION:

                            break;
                        case (int)PermitStatusEnum.PHYSICAL_INSPECTION_APPROVED:

                            break;
                        case (int)PermitStatusEnum.PHYSICAL_INSPECTION_REJECT:
                            emailTemplateID = 16;
                            break;
                        case (int)PermitStatusEnum.PAYMENT_OF_FEES:
                            emailTemplateID = 38; // email template id = 38 - Proceed to Payment
                            dateDueOfficer = null; //Next step is not assigned to the officer
                            break;
                        case (int)PermitStatusEnum.PAYMENT_EVALUATION:

                            break;
                        case (int)PermitStatusEnum.PAYMENT_EVALUATION_REJECT:
                            emailTemplateID = 16;
                            break;
                        case (int)PermitStatusEnum.PERMIT_APPROVAL:

                            break;
                        case (int)PermitStatusEnum.PERMIT_APPROVAL_REJECT:
                            emailTemplateID = 16;
                            break;
                        case (int)PermitStatusEnum.PERMIT_ISSUANCE:
                            emailTemplateID = 39; //Permit to Import (Approval)
                            registrationDateToBeChanged = true;
                            dateRegistration = DateTime.Now; //Permit will be considered registered once it has been approved
                            expirationDateToBeChanged = true;
                            dateExpiration = DateTime.Now.AddYears(3); //Permit to Expire after 3 years
                            dateDueOfficer = null; //Since task is done, no more due date for officer
                            break;
                        case (int)PermitStatusEnum.PENRO_FOCAL_APPROVAL:
                            break;
                        case (int)PermitStatusEnum.PENRO_FOCAL_APPROVAL_REJECT:
                            break;
                        case (int)PermitStatusEnum.PENRO_APPROVAL:
                            break;
                        case (int)PermitStatusEnum.PENRO_APPROVAL_REJECT:
                            break;
                        case (int)PermitStatusEnum.REGION_FOCAL_APPROVAL:
                            break;
                        case (int)PermitStatusEnum.REGION_FOCAL_APPROVAL_REJECT:
                            break;
                        case (int)PermitStatusEnum.REGION_APPROVAL:
                            break;
                        case (int)PermitStatusEnum.REGION_APPROVAL_REJECT:
                            break;
                    }

                    //if (viewMod.applicantViewModels.status <= 6) // Approval Process before payment
                    //{
                    //    if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                    //    {
                    //        stats = 6; //For Payment which also means that it is already approved by CENRO
                    //        emailTemplateID = 38;
                    //        // email template id = 38 - Proceed to Payment
                    //        dateDueOfficer = null; //Next step is not assigned to the officer
                    //    }
                    //    else //Inspector
                    //    {
                    //        stats = 3; // Approved (Inspector) - For Cenro Approval                                                       
                    //    }
                    //}
                    //else //Final Approval of Application including Payments
                    //{
                    //    if (Role == "DENR CENRO" || Role == "DENR Implementing PENRO" || Role == "DENR Regional Executive Director (RED)")
                    //    {
                    //        stats = 11; //Payment and Application Approved (Inspector and CENRO)
                    //        emailTemplateID = 39; //Permit to Import (Approval)
                    //        registrationDateToBeChanged = true;
                    //        dateRegistration = DateTime.Now; //Permit will be considered registered once it has been approved
                    //        expirationDateToBeChanged = true;
                    //        dateExpiration = DateTime.Now.AddYears(3); //Permit to Expire after 3 years
                    //        dateDueOfficer = null; //Since task is done, no more due date for officer
                    //    }
                    //    else //Inspector
                    //    {
                    //        stats = 9; // Payment Verification (CENRO) - Approved by Inspector and to be verified by CENRO
                    //    }
                    //}



                    var appli = new tbl_application() { id = applid, status = stats, date_modified = DateTime.Now, modified_by = loggedUserID, initial_date_of_inspection = dateInspectionInitial, date_of_inspection = dateInspection, date_of_registration = dateRegistration, date_of_expiration = dateExpiration, date_due_for_officers = dateDueOfficer };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        _context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_of_inspection).IsModified = true;
                        _context.Entry(appli).Property(x => x.initial_date_of_inspection).IsModified = initialInspectDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_inspection).IsModified = inspectDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_registration).IsModified = registrationDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_of_expiration).IsModified = expirationDateToBeChanged;
                        _context.Entry(appli).Property(x => x.date_due_for_officers).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();

                        //Log User Activity
                        var statsName = _context.tbl_permit_workflow_step.Where(pwfs => pwfs.permit_type_code == "1" && pwfs.workflow_step_code == stats.ToString()).Select(pwfs => pwfs.name).FirstOrDefault();
                        var referenceNo = viewMod.applicantViewModels.ReferenceNo;
                        LogUserActivity("ImporterApproval", "Application Status Moved", $"{referenceNo} status moved to {statsName}.", apkDateTime: DateTime.Now);
                    }
                    //Email
                    if (emailTemplateID != 0) //If emailTemplateID is 0, no email should be sent.
                    {
                        var emailTemplate = emailTemplates.Where(e => e.id == emailTemplateID).FirstOrDefault();

                        var subject = emailTemplate.email_subject;
                        var BODY = emailTemplate.email_content.Replace("{FirstName}", viewMod.applicantViewModels.first_name);
                        var body = BODY.Replace(Environment.NewLine, "<br/>");

                        EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                    }
                }
                //else
                //{
                //    var appli = new tbl_application() { id = applid, date_modified = DateTime.Now, modified_by = loggedUserID };
                //    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                //    if (viewMod.filesUpload != null)
                //    {
                //        foreach (var file in viewMod.filesUpload.Files)
                //        {
                //            var filesDB = new tbl_files();
                //            FileInfo fileInfo = new FileInfo(file.FileName);
                //            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/UserDocs");

                //            //create folder if not exist
                //            if (!Directory.Exists(path))
                //                Directory.CreateDirectory(path);


                //            string fileNameWithPath = Path.Combine(path, file.FileName);

                //            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                //            {
                //                file.CopyTo(stream);
                //            }
                //            filesDB.tbl_application_id = id;
                //            filesDB.created_by = usid;
                //            filesDB.modified_by = usid;
                //            filesDB.date_created = DateTime.Now;
                //            filesDB.date_modified = DateTime.Now;
                //            filesDB.filename = file.FileName;
                //            filesDB.path = path;
                //            filesDB.tbl_file_type_id = fileInfo.Extension;
                //            filesDB.tbl_file_sources_id = fileInfo.Extension;
                //            filesDB.file_size = Convert.ToInt32(file.Length);
                //            _context.tbl_files.Add(filesDB);
                //            _context.SaveChanges();
                //        }
                //    }
                //    using (_context)
                //    {
                //        _context.tbl_application.Attach(appli);
                //        //_context.Entry(appli).Property(x => x.status).IsModified = true;
                //        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                //        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                //        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                //        _context.SaveChanges();
                //    }
                //    //Email
                //    var subject = "Permit Application Status";
                //    var body = "Greetings! \n An inspector viewed your application.\nThe officer left the following comment:\n" + viewMod.applicantViewModels.comment;
                //    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                //}

                return RedirectToAction("ChainsawImporterApplicantsList", "ChainsawImporter");
            }

        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_approve_reject_applications")]
        public IActionResult ChainsawImporterApplicantsList()
        {
            //if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            //{
            //    return RedirectToAction("Index", "ChainsawImporter");
            //}
            //else
            //{
                int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                var userRegion = _context.tbl_user.Where(u=>u.id == loggedUserID).Select(u=>u.tbl_region_id).FirstOrDefault();

                ViewModel mymodel = new ViewModel();

                var applicationlist = from a in _context.tbl_application
                                          //where a.tbl_user_id == userID
                                      where a.tbl_application_type_id == 2
                                      select a;

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;

                var applicationMod = (from a in applicationlist
                                     join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                     join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                     join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                     join pS in _context.tbl_permit_status on a.status equals pS.id
                                    //  join pSS in _context.tbl_permit_statuses on a.status equals pSS.id
                                    //  join wf in _context.tbl_permit_workflow on pT.id.ToString() equals wf.permit_type_code
                                     join wfs in _context.tbl_permit_workflow_step on new { permitType = pT.id.ToString(), status = a.status.ToString() } equals new { permitType = wfs.permit_type_code, status = wfs.workflow_step_code } 
                                     where usr.tbl_region_id == userRegion
                                     //where a.tbl_user_id == userID
                                     let current_step_count = (int)Math.Ceiling((decimal)a.status / 2) // Soon be dynamic
                                     let current_max_count = usr.tbl_region_id == 13 ? 6 : 10// Soon be dynamic 
                                     select new ApplicantListViewModel
                                     {
                                         ReferenceNo = a.ReferenceNo,
                                         id = a.id, 
                                         applicationDate = a.date_created,
                                         qty = a.qty,
                                         full_name = usr.user_classification == "Individual"? usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix: usr.company_name,
                                         email = usr.email,
                                         contact = usr.contact_no,
                                         address = usr.street_address,
                                         application_type = appt.name,
                                         permit_type = pT.name,
                                         permit_statuses = wfs.name,
                                         tbl_user_id = (int)usr.id,
                                         date_due_for_officers = a.date_due_for_officers,
                                         currentStepCount = current_step_count,
                                         currentMaxCount = current_max_count,
                                         isRead = _context.tbl_application_read.Any(r => r.tbl_application_id == a.id && r.tbl_user_id == loggedUserID && r.is_read),
                                         currentPercentage = (current_step_count * 100 / current_max_count)
                                     }).ToList();
                                
                //bool isReadExist;
                //bool isAppRead;

                //for (int i = 0; i < applicationMod.Count(); i++)
                //{
                //    isReadExist = _context.tbl_application_read.Any(r => r.tbl_application_id == applicationMod[i].id && r.tbl_user_id == loggedUserID);
                //    if (isReadExist)
                //    {
                //        isAppRead = _context.tbl_application_read.Where(r => r.tbl_application_id == applicationMod[i].id && r.tbl_user_id == loggedUserID).Select(r => r.is_read).FirstOrDefault();
                //        if (isAppRead)
                //        {
                //            //true
                //            applicationMod[i].isRead = true;
                //        }
                //        else
                //        {
                //            //false
                //            applicationMod[i].isRead = false;
                //        }
                //    }
                //    else
                //    {
                //        //false
                //        applicationMod[i].isRead = false;
                //    }
                //}
                //foreach(ApplicantListViewModel mod in applicationMod) {
                //    mod.currentPercentage = (mod.currentStepCount * 100 / mod.currentMaxCount);
                //}
                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
                //return RedirectToAction("Index", "Home");
            //}

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

            //Log User Activity
            LogUserActivity("Comments", "New Comment", $"Added new comment on Application#{appid}", apkDateTime: DateTime.Now);

            //return RedirectToAction("AccountsApproval?uid="+uid, "AccountManagement");
            //Url.Action("A","B",new{a="x"})

            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("EditApplication", "Application", new { uid = uid, appid = appid });
            }
            else
            {
                return RedirectToAction("ChainsawImporterApproval", "ChainsawImporter", new { uid = uid, appid = model.appid });
            }
        }


        [HttpPost, ActionName("FileTaggingChanges")]
        public JsonResult FileTaggingChanges(int tbl_files_id, string tbl_files_status, string comment, int appid, int documentChecklistId)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string Role = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;

            // var fileDB = _context.tbl_files.Where(f => f.Id == tbl_files_id).FirstOrDefault();
            // fileDB.status = tbl_files_status;
            var m = _context.tbl_files_checklist_bridge.Where(d => d.tbl_document_checklist_id == documentChecklistId && d.tbl_files_id == tbl_files_id).FirstOrDefault();
            m.status = tbl_files_status;
            // m.status = tbl_files_comment;
            _context.SaveChanges();

            var commentsTbl = new tbl_comments();
            //commentsTbl.id = model.tbl_Comments.id;
            commentsTbl.tbl_application_id = Convert.ToInt32(appid);
            commentsTbl.tbl_files_id = tbl_files_id;
            //commentsTbl.comment_to = model.tbl_Comments.comment_to;
            commentsTbl.comment = comment;
            commentsTbl.created_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.modified_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.date_created = DateTime.Now;
            commentsTbl.date_modified = DateTime.Now;
            commentsTbl.bridge_id = m.id;
            _context.tbl_comments.Add(commentsTbl);
            _context.SaveChanges();

            //Log User Activity
            LogUserActivity("DocumentTags", "Document Tag Action", $"Document tag for file #{tbl_files_id} changed to {tbl_files_status}", apkDateTime: DateTime.Now);

            // _context.SaveChanges();

            return Json(true);
        }
        
        [HttpGet, ActionName("DocumentHistory")]
        public JsonResult DocumentHistory(int tbl_files_id)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string Role = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;

            //var commentsTbl = _context.tbl_comments.Where(c => c.tbl_files_id == tbl_files_id).ToList();
            //var fileDB = _context.tbl_files.Where(f => f.Id == tbl_files_id).FirstOrDefault();
            
            var documentHistoryDetails = (from c in _context.tbl_comments
                                          where c.tbl_files_id == tbl_files_id
                                          join usr in _context.tbl_user on c.created_by equals usr.id
                                          join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                          select new
                                          {
                                              f.filename,
                                              c.comment,
                                              commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                              c.date_created,
                                              formattedDate = c.date_created.ToString("yyyy MMMM dd hh:mm tt")
                                          }).OrderByDescending(d => d.date_created);

            return Json(documentHistoryDetails);
        }


        [HttpPost, ActionName("applicationRead")]
        public JsonResult applicationRead(int appid)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            bool isExist = _context.tbl_application_read.Any(r=>r.tbl_user_id == loggedUserID && r.tbl_application_id == appid);

            if (!isExist)
            {
                var applicRead = new tbl_application_read();
                applicRead.tbl_application_id = appid;
                applicRead.tbl_user_id = loggedUserID;
                applicRead.is_read = true;
                applicRead.date_created = Convert.ToDateTime(DateTime.Now);
                applicRead.date_modified = Convert.ToDateTime(DateTime.Now);
                _context.tbl_application_read.Add(applicRead);
                _context.SaveChanges();
            }
            
            return Json(true);
        }
        //file-delete
        [HttpPost, ActionName("filedelete")]
        public JsonResult filedelete()
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            
            return Json(true);
        }

    }
}
