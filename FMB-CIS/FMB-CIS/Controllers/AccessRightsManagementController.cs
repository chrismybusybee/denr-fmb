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

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class AccessRightsManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

        public AccessRightsManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        public void LogUserActivity(string entity, string userAction, string remarks, int userId = 0, string source = "Web", DateTime? apkDateTime = null)
        {
            try
            {
                int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

                if (entity.ToUpper() == "LOGOUT"
                    && source.ToUpper() == "WEB")
                {
                    var userFullName = _context.tbl_user.Where(x => x.id == uid)
                     .Select(y => y.FullName).FirstOrDefault();

                    remarks = "User logged out. Username: " + userFullName;
                }

     
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
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// AccessRights
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessRightsList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                LogUserActivity("AccessRightsList", "Access Rights List", "getting access rights list", apkDateTime : DateTime.Now);

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult AccessRightsListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                AccessRightsListViewModel model = new AccessRightsListViewModel();
                //Get the list of users
                var entities = _context.tbl_access_right.Where(e => e.is_active == true).ToList();
                model.accessRights = entities.Adapt<List<AccessRights>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;


                //LogUserActivity("AccessRightsListPartialView", "AccessRightsListPartialView", "partial view access", apkDateTime: DateTime.Now);
                return PartialView("~/Views/AccessRightsManagement/Manage/Partial/AccessRightsListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult AccessRightsCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                AccessRightsListViewModel model = new AccessRightsListViewModel();
                //Get the list of users
                var entities = _context.tbl_access_right.ToList();
                model.accessRights = entities.Adapt<List<AccessRights>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/AccessRightsCreateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult AccessRightsUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                AccessRights model = new AccessRights();
                //Get the list of users
                var entity = _context.tbl_access_right.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<AccessRights>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/AccessRightsUpdateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult AccessRightsDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                AccessRights model = new AccessRights();
                //Get the list of users
                var entity = _context.tbl_access_right.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<AccessRights>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/AccessRightsDeleteModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        [HttpPost]
        public IActionResult AccessRightsCreate(AccessRightsCreateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = new tbl_access_right();
                    entity.name = model.name;
                    entity.code = model.code;
                    entity.description = model.description;
                    entity.type = model.type;
                    entity.scope = model.scope;
                    entity.parent_code = model.parent_code;
                    entity.is_active = true;
                    entity.createdBy = uid;
                    entity.modifiedBy = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_access_right.Add(entity);

                    // 3. TO DO: Add logging / historical data

                    // 4. Save changes
                    _context.SaveChanges();

                    //LogUserActivity("AccessRightsCreate", "AccessRightsCreate", "Access Rights Created by user: " + uid);
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("AccessRightsManagement", "Access Rights Create", $"{entity.name} Access Rights has been created", apkDateTime: DateTime.Now);
                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }
        [HttpPut("AccessRightsManagement/AccessRightsUpdate/{id:int}")]
        public IActionResult AccessRightsUpdate(AccessRightsUpdateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = _context.tbl_access_right.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.name = model.name;
                    entity.code = model.code;
                    entity.description = model.description;
                    entity.type = model.type;
                    entity.scope = model.scope;
                    entity.parent_code = model.parent_code;
                    entity.is_active = true;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("AccessRightsManagement", "Access Rights Update", $"{entity.name} Access Rights has been updated", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }
        [HttpDelete("AccessRightsManagement/AccessRightsDelete/{id:int}")]
        public IActionResult AccessRightsDelete(OfficeDeleteViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = _context.tbl_access_right.Where(m => m.id == model.id).FirstOrDefault();

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
                    LogUserActivity("AccessRightsManagement", "Access Rights Delete", $"{entity.name} Access Rights has been deleted", apkDateTime: DateTime.Now);
                    
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }


        /// <summary>
        /// UserType
        /// </summary>
        /// <returns></returns>
        public IActionResult UserTypeList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult UserTypeListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserTypeListViewModel model = new UserTypeListViewModel();
                //Get the list of users
                var entities = _context.tbl_user_types.Where(e => e.is_active == true).ToList();
                model.userTypes = entities.Adapt<List<UserType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Partial/UserTypeListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult UserTypeCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserTypeListViewModel model = new UserTypeListViewModel();
                //Get the list of users
                var entities = _context.tbl_user_types.ToList();
                model.userTypes = entities.Adapt<List<UserType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/UserTypeCreateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult UserTypeUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserType model = new UserType();
                //Get the list of users
                var entity = _context.tbl_user_types.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<UserType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/UserTypeUpdateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult UserTypeDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserType model = new UserType();
                //Get the list of users
                var entity = _context.tbl_user_types.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<UserType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Modal/UserTypeDeleteModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        [HttpPost]
        public IActionResult UserTypeCreate(UserTypeCreateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = new tbl_user_types();
                    //entity.id = model.id;
                    entity.name = model.name;
                    entity.tbl_user_id = 0;
                    entity.is_active = true;
                    entity.created_by = uid;
                    entity.modified_by = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_user_types.Add(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("AccessRightsManagement", "User Type Create", $"{entity.name} User Type has been created" , apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }
        [HttpPut("AccessRightsManagement/UserTypeUpdate/{id:int}")]
        public IActionResult UserTypeUpdate(UserTypeUpdateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = _context.tbl_user_types.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.name = model.name;
                    entity.modified_by = uid;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("AccessRightsManagement", "User Type Update", $"{entity.name} User Type has been updated", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }
        [HttpDelete("AccessRightsManagement/UserTypeDelete/{id:int}")]
        public IActionResult UserTypeDelete(OfficeDeleteViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
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
                    var entity = _context.tbl_user_types.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.is_active = false;
                    entity.modified_by = uid;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("AccessRightsManagement", "User Type Delete", $"{entity.name} User Type has been deleted", apkDateTime: DateTime.Now);
                    // 4. Save changes
                    _context.SaveChanges();

                    // 5. Return result
                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }

        /// <summary>
        /// UserTypeAccessRights
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserTypeAccessRights> UserTypeAccessRights()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                var userTypeAccessRights = _context.tbl_user_type_access_right.Where(e => e.is_active == true).ToList();
                return userTypeAccessRights.Adapt<List<UserTypeAccessRights>>();
            }
            else
            {
                return null;
            }
        }
        public IActionResult UserTypeAccessRightsList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult UserTypeAccessRightsListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserTypeAccessRightsListViewModel model = new UserTypeAccessRightsListViewModel();
                //Get the list of user types
                var userTypes = _context.tbl_user_types.Where(e => e.is_active == true).ToList();
                model.userTypes = userTypes.Adapt<List<UserType>>();
                //Get the list of access rights
                var accessRights = _context.tbl_access_right.Where(e => e.is_active == true).ToList();
                model.accessRights = accessRights.Adapt<List<AccessRights>>();
                //Get the list 
                var userTypeAccessRights = _context.tbl_user_type_access_right.Where(e => e.is_active == true).ToList();
                model.userTypeAccessRights = userTypeAccessRights.Adapt<List<UserTypeAccessRights>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Partial/UserTypeAccessRightsListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public IActionResult UserTypeAccessRightsToggle(UserTypeAccessRightsToggleViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    var entity = _context.tbl_user_type_access_right.Where(m => m.user_type_id == model.userTypeId && m.access_right_id == model.accessRightsId).FirstOrDefault();

                    if (entity == null) // No settings yet, create
                    {
                        var userTypeAccessRightsEntity = new tbl_user_type_access_right();
                        userTypeAccessRightsEntity.user_type_id = model.userTypeId;
                        userTypeAccessRightsEntity.access_right_id = model.accessRightsId;
                        userTypeAccessRightsEntity.is_active = true;
                        userTypeAccessRightsEntity.created_by = uid;
                        userTypeAccessRightsEntity.modified_by = uid;
                        userTypeAccessRightsEntity.date_created = DateTime.Now;
                        userTypeAccessRightsEntity.date_modified = DateTime.Now;

                        _context.tbl_user_type_access_right.Add(userTypeAccessRightsEntity);

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var accessRight = _context.tbl_access_right.Where(x => x.id == userTypeAccessRightsEntity.access_right_id).FirstOrDefault();
                        var userType = _context.tbl_user_types.Where(x => x.id == userTypeAccessRightsEntity.user_type_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "UserType Access Rights Toggle", $"{accessRight?.name} for user {userType?.name} has been enabled", apkDateTime: DateTime.Now);
                    }
                    else
                    {
                        entity.is_active = !entity.is_active;
                        entity.modified_by = uid;
                        entity.date_modified = DateTime.Now;
                        _context.Update(entity);

                        var accessRightAction = string.Empty;
                        if (entity.is_active == true)
                        {
                            accessRightAction = "enabled";
                        }
                        else
                        {
                            accessRightAction = "disabled";
                        }

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var accessRight = _context.tbl_access_right.Where(x => x.id == entity.access_right_id).FirstOrDefault();
                        var userType = _context.tbl_user_types.Where(x => x.id == entity.user_type_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "User Type Access Rights Toggle", $"{accessRight?.name} for user {userType?.name} has been {accessRightAction}", apkDateTime: DateTime.Now);
                    }
                    _context.SaveChanges();

                    // TO DO : Add historical data

                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }

        //public IActionResult AccessRightsTypeCreateModal()
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        OfficeTypeListViewModel model = new OfficeTypeListViewModel();
        //        //Get the list of users
        //        //var entities = _context.tbl_office_type.ToList();
        //        //model.officeTypes = entities.Adapt<List<OfficeType>>();

        //        string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
        //        ViewData["BaseUrl"] = host;

        //        return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeCreateModal.cshtml", model);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //}
        //public IActionResult AccessRightsTypeUpdateModal(int id)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        OfficeType model = new OfficeType();
        //        //Get the list of users
        //        var entity = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
        //        model = entity.Adapt<OfficeType>();

        //        string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
        //        ViewData["BaseUrl"] = host;

        //        return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeUpdateModal.cshtml", model);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //}
        //public IActionResult AccessRightsTypeDeleteModal(int id)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        OfficeType model = new OfficeType();
        //        //Get the list of users
        //        var officeType = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
        //        model = officeType.Adapt<OfficeType>();

        //        string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
        //        ViewData["BaseUrl"] = host;

        //        return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeDeleteModal.cshtml", model);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //}

        //[HttpPost]
        //public IActionResult AccessRightsTypeCreate(OfficeTypeCreateViewModel model)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        // Uses fluent validation
        //        if (!ModelState.IsValid)
        //        {
        //            return StatusCode(StatusCodes.Status400BadRequest, ModelState);
        //        }
        //        else
        //        {
        //            // Save
        //            // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

        //            // Action: 1 to 5
        //            // 1. create entity
        //            var entity = new tbl_office_type();
        //            entity.name = model.name;
        //            entity.description = model.description;
        //            entity.is_active = true;
        //            entity.createdBy = uid;
        //            entity.modifiedBy = uid;
        //            entity.date_created = DateTime.Now;
        //            entity.date_modified = DateTime.Now;

        //            // 2. add to context
        //            _context.tbl_office_type.Add(entity);

        //            // 3. TO DO: Add logging / historical data

        //            // 4. Save changes
        //            _context.SaveChanges();

        //            // 5. Return result
        //            return StatusCode(StatusCodes.Status201Created, ModelState);
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "AccountManagement");
        //    }
        //}
        //[HttpPut("AccessRightsManagement/AccessRightsTypeUpdate/{id:int}")]
        //public IActionResult OfficeTypeUpdate(OfficeTypeUpdateViewModel model)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        // Uses fluent validation
        //        if (!ModelState.IsValid)
        //        {
        //            return StatusCode(StatusCodes.Status400BadRequest, ModelState);
        //        }
        //        else
        //        {
        //            // Save
        //            // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

        //            // Action: 1 to 5
        //            // 1. get, update entity
        //            var entity = _context.tbl_office_type.Where(m => m.id == model.id).FirstOrDefault();

        //            // NOTE: TO DO, is there a better location for this
        //            if(entity == null)
        //            {
        //                return StatusCode(StatusCodes.Status404NotFound, ModelState);
        //            }
        //            entity.name = model.name;
        //            entity.description = model.description;
        //            entity.modifiedBy = uid;
        //            entity.date_modified = DateTime.Now;

        //            // 2. update to context
        //            _context.Update(entity);

        //            // 3. TO DO: Add logging / historical data

        //            // 4. Save changes
        //            _context.SaveChanges();

        //            // 5. Return result
        //            return StatusCode(StatusCodes.Status201Created, ModelState);
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "AccountManagement");
        //    }
        //}
        //[HttpDelete("AccessRightsManagement/AccessRightsTypeDelete/{id:int}")]
        //public IActionResult OfficeTypeDelete(OfficeTypeDeleteViewModel model)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        // Uses fluent validation
        //        if (!ModelState.IsValid)
        //        {
        //            return StatusCode(StatusCodes.Status400BadRequest, ModelState);
        //        }
        //        else
        //        {
        //            // Save
        //            // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

        //            // Action: 1 to 5
        //            // 1. get, update entity
        //            var entity = _context.tbl_office_type.Where(m => m.id == model.id).FirstOrDefault();

        //            // NOTE: TO DO, is there a better location for this
        //            if (entity == null)
        //            {
        //                return StatusCode(StatusCodes.Status404NotFound, ModelState);
        //            }
        //            entity.is_active = false;
        //            entity.modifiedBy = uid;
        //            entity.date_modified = DateTime.Now;

        //            // 2. update to context
        //            _context.Update(entity);

        //            // 3. TO DO: Add logging / historical data

        //            // 4. Save changes
        //            _context.SaveChanges();

        //            // 5. Return result
        //            return StatusCode(StatusCodes.Status201Created, ModelState);
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "AccountManagement");
        //    }
        //}



        /// <summary>
        /// UserTypeUser
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserType> UserTypes()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                var userTypes = _context.tbl_user_types.Where(e => e.is_active == true).ToList();
                return userTypes.Adapt<List<UserType>>();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// UserTypeUser
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserTypeUser> UserTypeUsers()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                var userTypeUsers = _context.tbl_user_type_user.Where(e => e.is_active == true).ToList();
                return userTypeUsers.Adapt<List<UserTypeUser>>();
            }
            else
            {
                return null;
            }
        }
        public IActionResult UserTypeUserList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult UserTypeUserListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserTypeUserListViewModel model = new UserTypeUserListViewModel();
                //Get the list of user types
                var userTypes = _context.tbl_user_types.Where(e => e.is_active == true).ToList();
                model.userTypes = userTypes.Adapt<List<UserType>>();
                //Get the list of access rights
                var users = _context.tbl_user.Where(e => e.is_active == true).ToList();
                model.users = users.Adapt<List<User>>();
                //Get the list 
                var userTypeUsers = _context.tbl_user_type_user.Where(e => e.is_active == true).ToList();
                model.userTypeUsers = userTypeUsers.Adapt<List<UserTypeUser>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Partial/UserTypeUserListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public IActionResult UserTypeUserToggle(UserTypeUserToggleViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    var entity = _context.tbl_user_type_user.Where(m => m.user_type_id == model.userTypeId && m.user_id == model.userId).FirstOrDefault();

                    if (entity == null) // No settings yet, create
                    {
                        var UserTypeUserEntity = new tbl_user_type_user();
                        UserTypeUserEntity.user_type_id = model.userTypeId;
                        UserTypeUserEntity.user_id = model.userId;
                        UserTypeUserEntity.is_active = true;
                        UserTypeUserEntity.created_by = uid;
                        UserTypeUserEntity.modified_by = uid;
                        UserTypeUserEntity.date_created = DateTime.Now;
                        UserTypeUserEntity.date_modified = DateTime.Now;

                        _context.tbl_user_type_user.Add(UserTypeUserEntity);

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var userTypes = _context.tbl_user_types.Where(x => x.id == UserTypeUserEntity.user_type_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "User Type User Toggle", $"{userTypes?.name} has been created", apkDateTime: DateTime.Now);
                    }
                    else
                    {
                        entity.is_active = !entity.is_active;
                        entity.modified_by = uid;
                        entity.date_modified = DateTime.Now;
                        _context.Update(entity);

                        var userAccessAction = string.Empty;
                        if(entity.is_active == true)
                        {
                            userAccessAction = "enabled";
                        }
                        else
                        {
                            userAccessAction = "disabled";
                        }

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var userTypes = _context.tbl_user_types.Where(x => x.id == entity.user_type_id).FirstOrDefault();
                        var user = _context.tbl_user.Where(x => x.id == entity.user_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "User Type User Toggle", $"{userTypes?.name} has been {userAccessAction} to user {user?.email}", apkDateTime: DateTime.Now);
                    }
                    _context.SaveChanges();

                    // TO DO : Add historical data

                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }




        public IActionResult UserTypeUsersList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult UserTypeUsersListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (userRoleID == 14) // Super Admin
            {
                UserTypeUserListViewModel model = new UserTypeUserListViewModel();
                //Get the list of user types
                var userTypes = _context.tbl_user_types.Where(e => e.is_active == true).ToList();
                model.userTypes = userTypes.Adapt<List<UserType>>();
                //Get the list of access rights
                var users = _context.tbl_user.Where(e => e.is_active == true).ToList();
                model.users = users.Adapt<List<User>>();
                //Get the list 
                var userTypeUsers = _context.tbl_user_type_user.Where(e => e.is_active == true).ToList();
                model.userTypeUsers = userTypeUsers.Adapt<List<UserTypeUser>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/AccessRightsManagement/Manage/Partial/UserTypeAccessRightsListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public IActionResult UserTypeUsersToggle(UserTypeUserToggleViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (userRoleID == 14) // Super Admin
            {
                // Uses fluent validation
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    var entity = _context.tbl_user_type_user.Where(m => m.user_type_id == model.userTypeId && m.user_id == model.userId).FirstOrDefault();

                    if (entity == null) // No settings yet, create
                    {
                        var userTypeUserEntity = new tbl_user_type_user();
                        userTypeUserEntity.user_type_id = model.userTypeId;
                        userTypeUserEntity.user_id = model.userId;
                        userTypeUserEntity.is_active = true;
                        userTypeUserEntity.created_by = uid;
                        userTypeUserEntity.modified_by = uid;
                        userTypeUserEntity.date_created = DateTime.Now;
                        userTypeUserEntity.date_modified = DateTime.Now;

                        _context.tbl_user_type_user.Add(userTypeUserEntity);

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var userTypes = _context.tbl_user_types.Where(x => x.id == userTypeUserEntity.user_type_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "User Type Users Toggle", $"{userTypes?.name} User Type User has been enabled", apkDateTime: DateTime.Now);

                    }
                    else
                    {
                        entity.is_active = !entity.is_active;
                        entity.modified_by = uid;
                        entity.date_modified = DateTime.Now;
                        _context.Update(entity);

                        var userAccessAction = string.Empty;
                        if (entity.is_active == true)
                        {
                            userAccessAction = "enabled";
                        }
                        else
                        {
                            userAccessAction = "disabled";
                        }

                        var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                        var userTypes = _context.tbl_user_types.Where(x => x.id == entity.user_type_id).FirstOrDefault();
                        var user = _context.tbl_user.Where(x => x.id == entity.user_id).FirstOrDefault();
                        LogUserActivity("AccessRightsManagement", "User Type Users Toggle", $"{userTypes?.name} has been {userAccessAction} to user {user?.email}", apkDateTime: DateTime.Now);
                    }
                    _context.SaveChanges();

                    // TO DO : Add historical data

                    return StatusCode(StatusCodes.Status201Created, ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }
        }
    }
}
