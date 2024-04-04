using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Ajax.Utilities;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class BrandManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

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
        public BrandManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        /// <summary>
        /// Brands
        /// </summary>
        /// <returns></returns>
        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        public IActionResult BrandList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return View();
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        public IActionResult BrandListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                BrandListViewModel model = new BrandListViewModel();
                //Get the list of users
                var entities = _context.tbl_brands.Where(e => e.is_active == true).ToList();
                model.brands = entities.Adapt<List<Brand>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/BrandManagement/Brand/Partial/BrandListPartial.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        public IActionResult BrandCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                BrandListViewModel model = new BrandListViewModel();
                //Get the list of users
                //var entities = _context.tbl_office_type.ToList();
                //model.officeTypes = entities.Adapt<List<OfficeType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/BrandManagement/Brand/Modal/BrandCreateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        public IActionResult BrandUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                Brand model = new Brand();
                //Get the list of users
                var entity = _context.tbl_brands.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<Brand>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/BrandManagement/Brand/Modal/BrandUpdateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        public IActionResult BrandDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                Brand model = new Brand();
                //Get the list of users
                var brand = _context.tbl_brands.FirstOrDefault(o => o.id == id);
                model = brand.Adapt<Brand>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/BrandManagement/Brand/Modal/BrandDeleteModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        [HttpPost]
        public IActionResult BrandCreate(BrandCreateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            //if (usrRoleID == 14) // Super Admin
            //{
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    // Action: 1 to 5
                    // 1. create entity
                    var entity = new tbl_brand();
                    entity.name = model.name;
                    entity.description = model.description;
                    entity.is_active = true;
                    entity.createdBy = uid;
                    entity.modifiedBy = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_brands.Add(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("BrandManagement", "Create Brand", $"Brand {entity.name} has been created", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            //}
            //else
            //{
            //    return RedirectToAction("Index", "AccountManagement");
            //}
        }
        [HttpPut("BrandManagement/BrandUpdate/{id:int}")]
        public IActionResult BrandUpdate(BrandUpdateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            //if (usrRoleID == 14) // Super Admin
            //{
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    // Action: 1 to 5
                    // 1. get, update entity
                    var entity = _context.tbl_brands.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.name = model.name;
                    entity.description = model.description;
                    entity.modifiedBy = uid;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("BrandManagement", "Update Brand", $"Brand {entity.name} has been updated", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            //}
            //else
            //{
            //    return RedirectToAction("Index", "AccountManagement");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_brands")]
        [HttpDelete("BrandManagement/BrandDelete/{id:int}")]
        public IActionResult BrandDelete(BrandDeleteViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            //if (usrRoleID == 14) // Super Admin
            //{
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    // Action: 1 to 5
                    // 1. get, update entity
                    var entity = _context.tbl_brands.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.is_active = false;
                    entity.modifiedBy = uid;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("BrandManagement", "Delete Brand", $"Brand {entity.name} has been deleted", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            //}
            //else
            //{
            //    return RedirectToAction("Index", "AccountManagement");
            //}
        }
    }
}
