
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
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
using System.Web.Helpers;
using Microsoft.Extensions.Hosting;

namespace FMB_CIS.Controllers
{
    public class NewChainsawRegistrationController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment EnvironmentHosting;


        public NewChainsawRegistrationController(IConfiguration configuration, LocalContext context, IEmailSender emailSender, IWebHostEnvironment _environment)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            EnvironmentHosting = _environment;
        }
        [RequiresAccess(allowedAccessRights = "allow_page_create_chainsaw_registration,allow_page_create_other_permits")]
        public IActionResult Index()
        {
            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;

            //Set Roles who can access this page
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            bool? usrStatus = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault();

            //ViewModel model = new ViewModel();
            //Get list of required documents from tbl_announcement
            var requirements = _context.tbl_announcement.Where(a => a.id == 5).FirstOrDefault(); // id = 5 for Certificate of Registration Requirements
            //model.soloAnnouncement = requirements;
            ViewBag.RequiredDocsList = requirements.announcement_content;
            //End for required documents

            ViewModel model = new ViewModel();
            //Document Checklist
            var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == 13 && c.is_active == true).ToList();
            model.tbl_Document_Checklist = myChecklist;
            //End for Document Checklist

            if (usrStatus != true) //IF User is not yet approved by the admin.
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
            //if (usrRoleID == 3 || usrRoleID == 5 || usrRoleID == 6 || usrRoleID == 7)
            //{
            //    return View(model);
            //}
            //else if (usrRoleID == 8 || usrRoleID == 9 || usrRoleID == 10 || usrRoleID == 11 || usrRoleID == 17) //(((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("DENR") == true)
            //{

            //    return RedirectToAction("ChainsawOwnerApplicantsList", "ChainsawOwner");

            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ViewModel model)
        {

            List<tbl_chainsaw> myDeserializedObjList = (List<tbl_chainsaw>)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Dataxxx, typeof(List<tbl_chainsaw>));

            //try
            //{
            if (ModelState.IsValid)
            {

                int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                var usrDB = _context.tbl_user.Where(u => u.id == userID).FirstOrDefault();
                //DAL dal = new DAL();

                //Get email and subject from templates in DB
                var emailTemplates = _context.tbl_email_template.ToList();

                //SAVE permit application
                model.tbl_Application = new tbl_application();
                model.tbl_Application.tbl_application_type_id = 1; //Chainsaw Owner
                model.tbl_Application.status = 1;
                model.tbl_Application.tbl_user_id = userID;
                model.tbl_Application.tbl_permit_type_id = 13;
                model.tbl_Application.is_active = true;
                model.tbl_Application.created_by = userID;
                model.tbl_Application.modified_by = userID;
                model.tbl_Application.date_created = DateTime.Now;
                model.tbl_Application.date_modified = DateTime.Now;
                model.tbl_Application.date_due_for_officers = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
                _context.tbl_application.Add(model.tbl_Application);
                _context.SaveChanges();


                //SAVE to tbl_chainsaw
                foreach (tbl_chainsaw chainsaw in myDeserializedObjList) {

                    tbl_chainsaw newChainsaw = new tbl_chainsaw();
                    newChainsaw.user_id = userID;
                    newChainsaw.tbl_application_id = model.tbl_Application.id;
                    newChainsaw.status = "Owner";
                    newChainsaw.is_active = true;
                    newChainsaw.created_by = userID;
                    newChainsaw.modified_by = userID;
                    newChainsaw.date_created = DateTime.Now;
                    newChainsaw.date_modified = DateTime.Now;
                    newChainsaw.created_by = userID;
                    newChainsaw.modified_by = userID;

                    newChainsaw.chainsaw_date_of_registration = DateTime.Now;
                    newChainsaw.chainsaw_date_of_expiration = DateTime.Now.AddYears(3);

                    newChainsaw.supplier = chainsaw.supplier;
                    newChainsaw.Power = chainsaw.Power;
                    newChainsaw.Brand = chainsaw.Brand;
                    newChainsaw.Model = chainsaw.Model;
                    newChainsaw.Engine = chainsaw.Engine;
                    newChainsaw.chainsaw_serial_number = chainsaw.chainsaw_serial_number;
                    newChainsaw.gb = chainsaw.gb;
                    newChainsaw.specification = chainsaw.specification;
                    newChainsaw.purpose = chainsaw.purpose;
                    

                    if (chainsaw.Power == "Gas")
                    {
                        newChainsaw.watt = null;
                    }
                    else if (chainsaw.Power == "Electric" || chainsaw.Power == "Battery")
                    {
                        newChainsaw.hp = null;
                    }

                    _context.tbl_chainsaw.Add(newChainsaw);
                }

              
                _context.SaveChanges();

                int? appID = model.tbl_Application.id;

                //File Upload
                if (model.filesUpload != null)
                {
                    var folderName = userID + "_" + model.tbl_Application.id;
                    foreach (var file in model.filesUpload.Files)
                    {
                        var filesDB = new tbl_files();
                        FileInfo fileInfo = new FileInfo(file.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/" + folderName);

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
                        _context.tbl_files.Add(filesDB);
                        _context.SaveChanges();

                        //Matching of tbl_files to tbl_document_checklist
                        foreach (var item in model.fileChecklistViewModel)
                        {
                            foreach (var item2 in item.FileNames)
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
                }

                //Email                
                var emailTemplate = emailTemplates.Where(e => e.id == 5).FirstOrDefault(); //5 - Certificate of Ownership (Acknowledging Receipt)
                var subject = emailTemplate.email_subject;
                var BODY = emailTemplate.email_content.Replace("{FirstName}", usrDB.first_name);
                var body = BODY.Replace(Environment.NewLine, "<br/>");

                EmailSender.SendEmailAsync(((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value, subject, body);

                ModelState.Clear();
                ViewBag.Message = "Save Success";
                //Get list of required documents from tbl_announcement
                var requirements = _context.tbl_announcement.Where(a => a.id == 5).FirstOrDefault(); // id = 5 for Certificate of Registration Requirements
                                                                                                     //model.soloAnnouncement = requirements;
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;
                //Document Checklist
                var myChecklist = _context.tbl_document_checklist.Where(c => c.permit_type_id == 13 && c.is_active == true).ToList();
                model.tbl_Document_Checklist = myChecklist;
                //End for Document Checklist

                ViewBag.RequiredDocsList = requirements.announcement_content;
                //End for required documents
                return View(model);
            }
            return View(model);
            //}
            //catch
            //{
            //    return View(model);
            //    //return RedirectToAction("Index", "Dashboard");
            //}
        }

        [HttpPost, ActionName("CheckExistingSerialNumOnField")]
        public JsonResult CheckExistingSerialNumOnField(string serialNum)
        {
            var chainsawSerialNumExist = _context.tbl_chainsaw.Where(m => m.chainsaw_serial_number == serialNum).Any();
            return Json(chainsawSerialNumExist);
            
        }
    }
}
