using System;
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
using Microsoft.Ajax.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace FMB_CIS.Controllers
{
    [Authorize]
    public class ChecklistManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;

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

        public ChecklistManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
        }

        [RequiresAccess(allowedAccessRights = "allow_page_checklist_management")]
        public IActionResult Index()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();

            ChecklistManagementViewModel model = new ChecklistManagementViewModel();

            //if (usrRoleID == 14) // Super Admin
            //{
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                //var checklistDocs = (from dc in _context.tbl_document_checklist
                //                     join pt in _context.tbl_permit_type on dc.permit_type_id equals pt.id
                //                     select new ChecklistManagementModel
                //                     {
                //                         tbl_document_checklist_id = dc.id,
                //                         permit_type_id = dc.permit_type_id,
                //                         tbl_permit_type_name = pt.name,
                //                         tbl_document_checklist_name = dc.name,
                //                         tbl_document_checklist_description = dc.description,
                //                         is_active = dc.is_active

                //                     }).OrderBy(ptype => ptype.tbl_permit_type_name).ToList();

                //model.checklistManagementModels = checklistDocs;

                var permitTypesData = _context.tbl_permit_type.Where(p => p.is_active == true).ToList();
                model.tbl_Permit_Types = permitTypesData;
                return View(model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_checklist_management")]
        public IActionResult ChecklistOfDocumentsPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();

            ChecklistManagementViewModel model = new ChecklistManagementViewModel();

            //if (usrRoleID == 14) // Super Admin
            //{
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;


                var checklistDocs = (from dc in _context.tbl_document_checklist
                                     join pt in _context.tbl_permit_type on dc.permit_type_id equals pt.id
                                     select new ChecklistManagementModel
                                     {
                                         tbl_document_checklist_id = dc.id,
                                         permit_type_id = dc.permit_type_id,
                                         tbl_permit_type_name = pt.name,
                                         tbl_document_checklist_name = dc.name,
                                         tbl_document_checklist_description = dc.description,
                                         is_active = dc.is_active

                                     }).OrderBy(ptype => ptype.tbl_permit_type_name).ToList();

                model.checklistManagementModels = checklistDocs;

                var permitTypesData = _context.tbl_permit_type.Where(p => p.is_active == true).ToList();
                model.tbl_Permit_Types = permitTypesData;
                return PartialView("~/Views/ChecklistManagement/Partial/ChecklistPartial.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [HttpGet, ActionName("GetChecklistData")]
        public JsonResult GetChecklistData(int tbl_document_checklist_id)
        {
            var checklistData = (from dc in _context.tbl_document_checklist
                                 join pt in _context.tbl_permit_type on dc.permit_type_id equals pt.id
                                 where dc.id == tbl_document_checklist_id
                                 select new
                                 {
                                     dc.id,
                                     dc.permit_type_id,
                                     permitType = pt.name, //permit type name (ex. Permit to Import, etc.)
                                     dc.name,
                                     dc.description
                                 }).FirstOrDefault();

            //var permitTypesData = _context.tbl_permit_type.ToList();

            //var data = new { checklistData, permitTypesData };
            //return Json(data);
            return Json(checklistData);
        }

        [HttpPost, ActionName("DeleteDocReq")]
        public JsonResult DeleteDocReq(int tbl_document_checklist_id)
        {
            //var tblchcklstFromDB = _context.tbl_document_checklist.Where(dc => dc.id == tbl_document_checklist_id).FirstOrDefault();
            var tblchcklstFromDB = _context.tbl_document_checklist.Find(tbl_document_checklist_id);
            if(tblchcklstFromDB != null)
            {
                _context.Remove(tblchcklstFromDB);
            }
            _context.SaveChanges();

            var permitName = _context.tbl_permit_types.Where(pt => pt.id == tblchcklstFromDB.permit_type_id).Select(pt => pt.name).FirstOrDefault();
            LogUserActivity("ChecklistManagement", $"Requirement Deleted", $"\"{tblchcklstFromDB.name}\" requirement for {permitName} has been deleted", apkDateTime: DateTime.Now);
            return Json(true);
        }
        [HttpPost, ActionName("EnableDisableDocReq")]
        public JsonResult EnableDisableDocReq(int tbl_document_checklist_id, bool enableOrDisable)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var tblchcklstFromDB = _context.tbl_document_checklist.Where(dc => dc.id == tbl_document_checklist_id).FirstOrDefault();

            tblchcklstFromDB.is_active = enableOrDisable;
            tblchcklstFromDB.date_modified = DateTime.Now;
            tblchcklstFromDB.modifiedBy = uid;
            _context.SaveChanges();

            var permitName = _context.tbl_permit_types.Where(pt=>pt.id == tblchcklstFromDB.permit_type_id).Select(pt=>pt.name).FirstOrDefault();
            LogUserActivity("ChecklistManagement", $"Requirement {(enableOrDisable == true? "Enabled": "Disabled")}", $"\"{tblchcklstFromDB.name}\" requirement for {permitName} has been set to {(enableOrDisable == true ? "Enabled" : "Disabled")}", apkDateTime: DateTime.Now);
            return Json(true);
        }

        //INACTIVE. Currently using the "EnableDisableDocReq"
        [HttpPost, ActionName("EnableDocReq")]
        public JsonResult EnableDocReq(int tbl_document_checklist_id)
        {

            var tblchcklstFromDB = _context.tbl_document_checklist.Where(dc => dc.id == tbl_document_checklist_id).FirstOrDefault();
            tblchcklstFromDB.is_active = true;
            _context.SaveChanges();
            return Json(true);
        }

        //INACTIVE. Currently using the "EnableDisableDocReq"
        [HttpPost, ActionName("DisableDocReq")]
        public JsonResult DisableDocReq(int tbl_document_checklist_id)
        {

            var tblchcklstFromDB = _context.tbl_document_checklist.Where(dc => dc.id == tbl_document_checklist_id).FirstOrDefault();
            tblchcklstFromDB.is_active = false;
            _context.SaveChanges();
            return Json(true);
        }


        [HttpPost, ActionName("EditChecklistInfo")]
        public IActionResult EditChecklistInfo(ChecklistManagementViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var tblchcklstFromDB = _context.tbl_document_checklist.Where(dc => dc.id == model.tbl_document_Checklist.id).FirstOrDefault();
            
            tblchcklstFromDB.permit_type_id = model.tbl_document_Checklist.permit_type_id;
            tblchcklstFromDB.name = model.tbl_document_Checklist.name;
            tblchcklstFromDB.description = model.tbl_document_Checklist.description;
            tblchcklstFromDB.modifiedBy = uid;
            tblchcklstFromDB.date_modified = DateTime.Now;
            _context.SaveChanges();
            var permitName = _context.tbl_permit_types.Where(pt => pt.id == tblchcklstFromDB.permit_type_id).Select(pt => pt.name).FirstOrDefault();
            LogUserActivity("ChecklistManagement", $"Requirement Updated", $"\"{tblchcklstFromDB.name}\" requirement for {permitName} has been updated.", apkDateTime: DateTime.Now);
            return RedirectToAction("Index", "ChecklistManagement");
        }

        [HttpPost, ActionName("CreateReqDocChecklist")]
        public IActionResult CreateReqDocChecklist(ChecklistManagementViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            model.tbl_document_Checklist.is_active = true;
            model.tbl_document_Checklist.createdBy = uid;
            model.tbl_document_Checklist.modifiedBy = uid;
            model.tbl_document_Checklist.date_created = DateTime.Now;
            model.tbl_document_Checklist.date_modified = DateTime.Now;

            _context.tbl_document_checklist.Add(model.tbl_document_Checklist);
            _context.SaveChanges();

            var permitName = _context.tbl_permit_types.Where(pt => pt.id == model.tbl_document_Checklist.permit_type_id).Select(pt => pt.name).FirstOrDefault();
            LogUserActivity("ChecklistManagement", $"Requirement Created", $"\"{model.tbl_document_Checklist.name}\" requirement for {permitName} has been created.", apkDateTime: DateTime.Now);
            return RedirectToAction("Index", "ChecklistManagement");
        }


    }
}
