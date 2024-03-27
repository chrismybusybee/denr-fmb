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
    public class OfficeManagementController : Controller
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

        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        public IActionResult OfficeList()
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

        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        public IActionResult OfficeListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeListViewModel model = new OfficeListViewModel();
                //Get the list of users
                var entities = _context.tbl_division.Where(e => e.is_active == true).ToList();
                
                model.offices = entities.Adapt<List<Office>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Partial/OfficeListPartial.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        public IActionResult OfficeCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeCreateViewModel model = new OfficeCreateViewModel();

                var _officeTypes = _context.tbl_office_type.Where(o => o.is_active == true).ToList();
                var _regions = _context.tbl_region.ToList();
                var _provinces = new List<tbl_province>();
                var _cities = new List<tbl_city>();

                _officeTypes.Add(new tbl_office_type() { id = 0, name = "--Select Department--" });
                _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
                _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
                _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });

                ViewData["OfficeTypeData"] = new SelectList(_officeTypes.OrderBy(s => s.id), "id", "name");
                ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");
                if (ViewData["ProvinceData"] == null)
                {
                    ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.id), "id", "name");
                }
                if (ViewData["CityData"] == null)
                {
                    ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.id), "id", "name");
                }
                //if (ViewData["BrgyData"] == null)
                //{
                //    ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.id), "id", "name");
                //}

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeCreateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        public IActionResult OfficeUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                Office model = new Office();
                //Get the list of users
                var entity = _context.tbl_division.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<Office>();

                var _officeTypes = _context.tbl_office_type.Where(o => o.is_active == true).ToList();
                var _regions = _context.tbl_region.ToList();
                var _provinces = _context.tbl_province.ToList();
                var _cities = _context.tbl_city.ToList();

                _officeTypes.Add(new tbl_office_type() { id = 0, name = "--Select Department--" });
                _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
                _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
                _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });

                ViewData["OfficeTypeData"] = new SelectList(_officeTypes.OrderBy(s => s.id), "id", "name");
                ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");
                ViewData["ProvinceData"] = new SelectList(_provinces.Where(p => p.regCode == model.region_id || p.id == 0).OrderBy(s => s.id), "id", "name");
                ViewData["CityData"] = new SelectList(_cities.Where(c => c.citymunCode == model.city_id || c.id == 0).OrderBy(s => s.id), "id", "name");
                if (ViewData["ProvinceData"] == null)
                {
                    ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.id), "id", "name");
                }
                if (ViewData["CityData"] == null)
                {
                    ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.id), "id", "name");
                }


                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeUpdateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        public IActionResult OfficeDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                Office model = new Office();
                //Get the list of users
                var office = _context.tbl_division.FirstOrDefault(o => o.id == id);
                model = office.Adapt<Office>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/Office/Modal/OfficeDeleteModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        [HttpPost]
        public IActionResult OfficeCreate(OfficeCreateViewModel model)
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
                    var entity = new tbl_division();
                    entity.office_name = model.office_name;
                    entity.department = model.department;
                    entity.region_id = model.region_id;
                    entity.province_id = model.province_id;
                    entity.city_id = model.city_id;
                    entity.company_name = model.company_name;
                    entity.is_active = true;
                    entity.created_by = uid;
                    entity.modified_by = uid;
                    entity.date_created = DateTime.Now;
                    entity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_division.Add(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Create Office", $"Office {entity.office_name} has been created", apkDateTime: DateTime.Now);
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
        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        [HttpPut("OfficeManagement/OfficeUpdate/{id:int}")]
        public IActionResult OfficeUpdate(OfficeUpdateViewModel model)
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
                    entity.city_id = model.city_id;
                    entity.company_name = model.company_name;
                    entity.date_modified = DateTime.Now;

                    // 2. update to context
                    _context.Update(entity);

                    // 3. TO DO: Add logging / historical data
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Update Office", $"Office {entity.office_name} has been updated", apkDateTime: DateTime.Now);
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

        [RequiresAccess(allowedAccessRights = "allow_page_manage_offices")]
        [HttpDelete("OfficeManagement/OfficeDelete/{id:int}")]
        public IActionResult OfficeDelete(OfficeDeleteViewModel model)
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
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Delete Office", $"Office {entity.office_name} has been deleted", apkDateTime: DateTime.Now);
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

        /// <summary>
        /// Office Types
        /// </summary>
        /// <returns></returns>
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        public IActionResult OfficeTypeList()
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
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        public IActionResult OfficeTypeListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeTypeListViewModel model = new OfficeTypeListViewModel();
                //Get the list of users
                var entities = _context.tbl_office_type.Where(e => e.is_active == true).ToList();
                model.officeTypes = entities.Adapt<List<OfficeType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Partial/OfficeTypeListPartial.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        public IActionResult OfficeTypeCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeTypeListViewModel model = new OfficeTypeListViewModel();
                //Get the list of users
                //var entities = _context.tbl_office_type.ToList();
                //model.officeTypes = entities.Adapt<List<OfficeType>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeCreateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        public IActionResult OfficeTypeUpdateModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeType model = new OfficeType();
                //Get the list of users
                var entity = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<OfficeType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeUpdateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        public IActionResult OfficeTypeDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                OfficeType model = new OfficeType();
                //Get the list of users
                var officeType = _context.tbl_office_type.FirstOrDefault(o => o.id == id);
                model = officeType.Adapt<OfficeType>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/OfficeManagement/OfficeType/Modal/OfficeTypeDeleteModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        [HttpPost]
        public IActionResult OfficeTypeCreate(OfficeTypeCreateViewModel model)
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
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Create Office Type", $"Office type {entity.name} has been created", apkDateTime: DateTime.Now);
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
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        [HttpPut("OfficeManagement/OfficeTypeUpdate/{id:int}")]
        public IActionResult OfficeTypeUpdate(OfficeTypeUpdateViewModel model)
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
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Update Office Type", $"Office type {entity.name} has been updated", apkDateTime: DateTime.Now);
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
        [RequiresAccess(allowedAccessRights = "allow_page_manage_office_types")]
        [HttpDelete("OfficeManagement/OfficeTypeDelete/{id:int}")]
        public IActionResult OfficeTypeDelete(OfficeTypeDeleteViewModel model)
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
                    var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                    LogUserActivity("OfficeManagement", "Delete Office Type", $"Office type {entity.name} has been deleted", apkDateTime: DateTime.Now);
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

        [HttpPost, ActionName("GetDivisionByUserType")]
        public JsonResult GetDivisionByUserType(string userTypeId)
        {
            int utID;
            List<tbl_division> divisionLists = new List<tbl_division>();
            if (!string.IsNullOrEmpty(userTypeId))
            {
                utID = Convert.ToInt32(userTypeId);
                if(utID == 17) // Soon update to dynamic RED
                {
                    divisionLists = _context.tbl_division.Where(d => d.department.Equals(1) && d.is_active == true).OrderBy(d => d.office_name).ToList();
                }
                else if (utID == 10) // PENRO
                {
                    divisionLists = _context.tbl_division.Where(d => d.department.Equals(2) && d.is_active == true).OrderBy(d => d.office_name).ToList();
                }
                else if (utID == 8) // CENRO
                {
                    divisionLists = _context.tbl_division.Where(d => d.department.Equals(3) && d.is_active == true).OrderBy(d => d.office_name).ToList();
                }
                else if (utID == 9) // Implementing PENRO
                {
                    divisionLists = _context.tbl_division.Where(d => d.department.Equals(4) && d.is_active == true).OrderBy(d => d.office_name).ToList();
                }
            }
            return Json(divisionLists);
        }

        [HttpPost, ActionName("GetRegions")]
        public JsonResult GetRegions()
        {
            List<tbl_region> regionLists = new List<tbl_region>();

            regionLists = _context.tbl_region.OrderBy(r => r.id).ToList();

            return Json(regionLists);
        }

    }
}
