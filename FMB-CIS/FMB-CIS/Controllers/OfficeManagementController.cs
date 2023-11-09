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
    public class OfficeManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

        public OfficeManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        /// <summary>
        /// Offices
        /// </summary>
        /// <returns></returns>
        //public IActionResult OfficeList()
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

        //        return View("OfficeList", model);
        //    }
        //}

        public IActionResult OfficeList()
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

        public IActionResult OfficeListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeListViewModel model = new OfficeListViewModel();
                //Get the list of users
                var entities = _context.tbl_division.Where(e => e.is_active == true).ToList();
                model.offices = entities.Adapt<List<Office>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Partial/OfficeListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }


        public IActionResult OfficeCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeListViewModel model = new OfficeListViewModel();
                //Get the list of users
                //var entities = _context.tbl_division.ToList();
                //model.offices = entities.Adapt<List<Office>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeCreateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult OfficeUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                Office model = new Office();
                //Get the list of users
                var entity = _context.tbl_division.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<Office>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeUpdateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult OfficeDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                Office model = new Office();
                //Get the list of users
                var office = _context.tbl_division.FirstOrDefault(o => o.id == id);
                model = office.Adapt<Office>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeDeleteModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public IActionResult OfficeCreate(OfficeCreateViewModel model)
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
                    var entity = new tbl_division();
                    entity.office_name = model.office_name;
                    entity.department = model.department;
                    entity.region_id = model.region_id;
                    entity.province_id = model.province_id;
                    entity.company_name = model.company_name;
                    entity.is_active = true;
                    entity.created_by = uid;
                    entity.modified_by = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_division.Add(entity);

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
        [HttpPut("OfficeManagement/OfficeUpdate/{id:int}")]
        public IActionResult OfficeUpdate(OfficeUpdateViewModel model)
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
                    var entity = _context.tbl_division.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if (entity == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, ModelState);
                    }
                    entity.office_name = model.office_name;
                    entity.department = model.department;
                    entity.region_id = model.region_id;
                    entity.province_id = model.province_id;
                    entity.company_name = model.company_name;
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
        [HttpDelete("OfficeManagement/OfficeDelete/{id:int}")]
        public IActionResult OfficeDelete(OfficeDeleteViewModel model)
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
                    var entity = _context.tbl_division.Where(m => m.id == model.id).FirstOrDefault();

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
        /// Office Types
        /// </summary>
        /// <returns></returns>
        public IActionResult OfficeTypeList()
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

        public IActionResult OfficeTypeListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeTypeListViewModel model = new OfficeTypeListViewModel();
                //Get the list of users
                var entities = _context.tbl_office_type.Where(e => e.is_active == true).ToList();
                model.officeTypes = entities.Adapt<List<OfficeType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Partial/OfficeTypeListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }


        public IActionResult OfficeTypeCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeTypeListViewModel model = new OfficeTypeListViewModel();
                //Get the list of users
                //var entities = _context.tbl_office_type.ToList();
                //model.officeTypes = entities.Adapt<List<OfficeType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeCreateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult OfficeTypeUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeType model = new OfficeType();
                //Get the list of users
                var entity = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<OfficeType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeUpdateModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult OfficeTypeDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                OfficeType model = new OfficeType();
                //Get the list of users
                var officeType = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
                model = officeType.Adapt<OfficeType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeDeleteModal.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public IActionResult OfficeTypeCreate(OfficeTypeCreateViewModel model)
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
                    var entity = new tbl_office_type();
                    entity.name = model.name;
                    entity.description = model.description;
                    entity.is_active = true;
                    entity.createdBy = uid;
                    entity.modifiedBy = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_office_type.Add(entity);

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
        [HttpPut("OfficeManagement/OfficeTypeUpdate/{id:int}")]
        public IActionResult OfficeTypeUpdate(OfficeTypeUpdateViewModel model)
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
                    var entity = _context.tbl_office_type.Where(m => m.id == model.id).FirstOrDefault();

                    // NOTE: TO DO, is there a better location for this
                    if(entity == null)
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
        [HttpDelete("OfficeManagement/OfficeTypeDelete/{id:int}")]
        public IActionResult OfficeTypeDelete(OfficeTypeDeleteViewModel model)
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
                    var entity = _context.tbl_office_type.Where(m => m.id == model.id).FirstOrDefault();

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
    }
}
