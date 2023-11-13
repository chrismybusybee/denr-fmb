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

        /// <summary>
        /// AccessRights
        /// </summary>
        /// <returns></returns>
        //public IActionResult AccessRightsList()
        //{
        //    if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
        //    {
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //    else
        //    {

        //        ViewModel model = new ViewModel();
        //        //Get the list of users
        //        var userList = _context.tbl_user.ToList();
        //        model.tbl_Users = userList;

        //        return View("AccessRightsList", model);
        //    }
        //}
        public IActionResult AccessRightsList()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
        public IActionResult AccessRightsListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                AccessRightsListViewModel model = new AccessRightsListViewModel();
                //Get the list of users
                var entities = _context.tbl_access_right.Where(e => e.is_active == true).ToList();
                model.accessRights = entities.Adapt<List<AccessRights>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (usrRoleID == 14) // Super Admin
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
        /// AccessRights Types
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserTypeAccessRights> UserTypeAccessRights()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                UserTypeAccessRightsListViewModel model = new UserTypeAccessRightsListViewModel();
                //Get the list of user types
                var userTypes = _context.tbl_user_types.ToList();
                model.userTypes = userTypes.Adapt<List<UserTypes>>();
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
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            if (usrRoleID == 14) // Super Admin
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
                    }
                    else
                    {
                        entity.is_active = !entity.is_active;
                        entity.modified_by = uid;
                        entity.date_modified = DateTime.Now;
                        _context.Update(entity);
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (usrRoleID == 14) // Super Admin
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (usrRoleID == 14) // Super Admin
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (usrRoleID == 14) // Super Admin
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (usrRoleID == 14) // Super Admin
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (usrRoleID == 14) // Super Admin
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
        //    int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
        //    if (usrRoleID == 14) // Super Admin
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
    }
}
