﻿using FMB_CIS.Data;
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
using WebGrease.Css.Extensions;

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class WorkflowManagementController : Controller
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
        public WorkflowManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        /// <summary>
        /// Workflow List
        /// </summary>
        /// <returns></returns>
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        public IActionResult WorkflowList()
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
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        public IActionResult WorkflowListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                WorkflowListViewModel model = new WorkflowListViewModel();
                //Get the list of users
                var entities = _context.tbl_permit_workflow.Where(e => e.is_active == true).ToList();
                model.workflows = entities.Adapt<List<Workflow>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/WorkflowManagement/Manage/Partial/WorkflowListPartial.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        public IActionResult WorkflowCreateModal()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                AccessRightsListViewModel model = new AccessRightsListViewModel();
                //Get the list of users
                var entities = _context.tbl_access_right.ToList();
                model.accessRights = entities.Adapt<List<AccessRights>>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/WorkflowManagement/Manage/Modal/WorkflowCreateModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        public IActionResult WorkflowUpdateModal(int id)
        {
            // security check, data loading on GetWorkflow
            WorkflowUpdateViewModel model = new WorkflowUpdateViewModel();
            model.id = id;

            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;

            return PartialView("~/Views/WorkflowManagement/Manage/Modal/WorkflowUpdateModal.cshtml", model);
        }
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        public IActionResult WorkflowDeleteModal(int id)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (usrRoleID == 14) // Super Admin
            //{
                Workflow model = new Workflow();
                //Get the list of users
                var entity = _context.tbl_permit_workflow.FirstOrDefault(o => o.id == id);
                model = entity.Adapt<Workflow>();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/WorkflowManagement/Manage/Modal/WorkflowDeleteModal.cshtml", model);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Dashboard");
            //}
        }
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        [HttpPost]
        public IActionResult WorkflowCreate(WorkflowCreateViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            // Note: TO DO Soon move Authorization to middleware / Annotations / User Access instead of Roles
            //if (usrRoleID == 14) // Super Admin
            //{
                // Uses fluent validation
                if (!ModelState.IsValid && false)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }
                else
                {
                    // Save
                    // Note: TO DO Soon refactor code, let's move the Data Access to a new layer

                    // Action: 1 to 5
                    string logActionName = "Created";
                    // Remove if existing
                    if (model.id != 0)
                    {
                        var removeEntity = _context.tbl_permit_workflow.Where(m => m.id == model.id).SingleOrDefault();
                        string workflowCode = removeEntity.workflow_code;
                        _context.tbl_permit_workflow.Remove(removeEntity);

                        var removeWorkflowStepEntity = _context.tbl_permit_workflow_step.Where(m => m.workflow_code == workflowCode);
                        _context.tbl_permit_workflow_step.RemoveRange(removeWorkflowStepEntity);

                        var removeWorkflowNextStepEntity = _context.tbl_permit_workflow_next_step.Where(m => m.workflow_code == workflowCode);
                        _context.tbl_permit_workflow_next_step.RemoveRange(removeWorkflowNextStepEntity);
                        logActionName = "Updated";
                    }

                    // 1. create entity
                    var workflowEntity = new tbl_permit_workflow();
                    workflowEntity.name = model.name;
                    workflowEntity.permit_type_code = model.permit_type_code;
                    workflowEntity.description = model.description;
                    workflowEntity.workflow_code = model.workflow_code;
                    workflowEntity.workflow_id = Guid.NewGuid().ToString();
                    //entity.workflowEntity_id = model.workflowEntity_id;
                    workflowEntity.is_active = true;
                    workflowEntity.createdBy = uid;
                    workflowEntity.modifiedBy = uid;
                    workflowEntity.date_created = DateTime.Now;
                    workflowEntity.date_modified = DateTime.Now;

                    // 2. add to context
                    _context.tbl_permit_workflow.Add(workflowEntity);

                    //var workflowStepId = Guid.NewGuid().ToString();

                    model.steps.ForEach(step =>
                    {
                        var workflowStepEntity = new tbl_permit_workflow_step();
                        workflowStepEntity.workflow_id = workflowEntity.workflow_id;
                        workflowStepEntity.workflow_step_id = Guid.NewGuid().ToString();
                        workflowStepEntity.permit_type_code = workflowEntity.permit_type_code;
                        workflowStepEntity.workflow_code = workflowEntity.workflow_code;
                        workflowStepEntity.workflow_step_code = step.workflow_step_code;
                        workflowStepEntity.permit_page_code = step.permit_page_code;
                        workflowStepEntity.name = step.name;
                        workflowStepEntity.description = step.description;
                        workflowStepEntity.on_pre_action = step.on_pre_action;
                        workflowStepEntity.on_success_action = step.on_success_action;
                        workflowStepEntity.on_exit_action = step.on_exit_action;
                        workflowStepEntity.is_active = true;
                        workflowStepEntity.createdBy = uid;
                        workflowStepEntity.modifiedBy = uid;
                        workflowStepEntity.date_created = DateTime.Now;
                        workflowStepEntity.date_modified = DateTime.Now;

                        // 2. add to context
                        _context.tbl_permit_workflow_step.Add(workflowStepEntity);

                        step.nextSteps.ForEach(nextStep =>
                        {
                            var workflowNextStepEntity = new tbl_permit_workflow_next_step();
                            workflowNextStepEntity.workflow_next_step_id = Guid.NewGuid().ToString();
                            workflowNextStepEntity.workflow_step_id = workflowStepEntity.workflow_step_id;
                            workflowNextStepEntity.workflow_id = workflowEntity.workflow_id;
                            workflowNextStepEntity.workflow_code = workflowEntity.workflow_code;
                            workflowNextStepEntity.workflow_step_code = workflowStepEntity.workflow_step_code;
                            workflowNextStepEntity.next_step_code = nextStep.next_step_code;
                            workflowNextStepEntity.division_parameter = nextStep.division_parameter;
                            workflowNextStepEntity.division_id = nextStep.division_id;
                            workflowNextStepEntity.user_type_id = nextStep.user_type_id;
                            workflowNextStepEntity.button_text = nextStep.button_text;
                            workflowNextStepEntity.button_class = nextStep.button_class;
                            workflowNextStepEntity.is_active = true;
                            workflowNextStepEntity.createdBy = uid;
                            workflowNextStepEntity.modifiedBy = uid;
                            workflowNextStepEntity.date_created = DateTime.Now;
                            workflowNextStepEntity.date_modified = DateTime.Now;

                            // 2. add to context
                            _context.tbl_permit_workflow_next_step.Add(workflowNextStepEntity);
                        });
                    });

                    // 3. Save changes

                    // Remove previous
                    _context.SaveChanges();

                    // 4. Add logging / historical data
                    LogUserActivity("WorkflowManagement", $"Workflow {logActionName}", $"{model.name} workflow {logActionName.ToLower()}", apkDateTime: DateTime.Now);

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
        /// 
        /// UserTypeUser
        /// </summary>
        /// <returns></returns>
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        [HttpGet("WorkflowManagement/GetWorkflow/{workflowId:int}")]
        public Workflow GetWorkflow(int workflowId)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            //if (userRoleID == 14) // Super Admin
            //{
                Workflow model = new Workflow();
                //Get the list of users
                var workflow = _context.tbl_permit_workflow.FirstOrDefault(e => e.id == workflowId);
                model = workflow.Adapt<Workflow>();

                //Get the list of steps
                var stepEntity = _context.tbl_permit_workflow_step.Where(o => o.workflow_code == model.workflow_code).ToList();
                model.steps = stepEntity.Adapt<List<WorkflowStep>>();

                //Get the list of nextsteps
                foreach (WorkflowStep workflowStep in model.steps)
                {
                    var nextstepEntity = _context.tbl_permit_workflow_next_step.Where(o => o.workflow_code == model.workflow_code && o.workflow_step_code == workflowStep.workflow_step_code && o.workflow_step_id == workflowStep.workflow_step_id).ToList();
                    //o.workflow_code == model.workflow_code &&
                    workflowStep.nextSteps = nextstepEntity.Adapt<List<WorkflowNextStep>>();
                }
                return model;
            //}
            //else
            //{
            //    return null;
            //}
        }

        ///// <summary>
        ///// UserTypeUser
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("WorkflowManagement/GetWorkflowByCode/{workflowCode:String}")]
        //public Workflow GetWorkflowByCode(string workflowCode)
        //{
        //    int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
        //    int userRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
        //    if (userRoleID == 14) // Super Admin
        //    {
        //        Workflow model = new Workflow();
        //        //Get the list of users
        //        var workflow = _context.tbl_permit_workflow.FirstOrDefault(e => e.workflow_code == workflowCode);
        //        model = workflow.Adapt<Workflow>();

        //        //Get the list of steps
        //        var stepEntity = _context.tbl_permit_workflow_step.Where(o => o.workflow_code == model.workflow_code).ToList();
        //        model.steps = stepEntity.Adapt<List<WorkflowStep>>();

        //        //Get the list of nextsteps
        //        foreach (WorkflowStep workflowStep in model.steps)
        //        {
        //            var nextstepEntity = _context.tbl_permit_workflow_next_step.Where(o => o.workflow_step_code == workflowStep.workflow_step_code).ToList();
        //            //o.workflow_code == model.workflow_code &&
        //            workflowStep.nextSteps = nextstepEntity.Adapt<List<WorkflowNextStep>>();
        //        }
        //        return model;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}


        [HttpPut("WorkflowManagement/WorkflowUpdate/{id:int}")]
        public IActionResult WorkflowUpdate(AccessRightsUpdateViewModel model)
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
        [RequiresAccess(allowedAccessRights = "allow_page_workflow_management")]
        [HttpDelete("WorkflowManagement/WorkflowDelete/{id:int}")]
        public IActionResult WorkflowDelete(OfficeDeleteViewModel model)
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
                    string workflowName = "";
                    if (model.id != 0)
                    {
                        var removeEntity = _context.tbl_permit_workflow.Where(m => m.id == model.id).SingleOrDefault();
                        string workflowCode = removeEntity.workflow_code;
                        workflowName = removeEntity.name;
                        _context.tbl_permit_workflow.Remove(removeEntity);

                        var removeWorkflowStepEntity = _context.tbl_permit_workflow_step.Where(m => m.workflow_code == workflowCode);
                        _context.tbl_permit_workflow_step.RemoveRange(removeWorkflowStepEntity);

                        var removeWorkflowNextStepEntity = _context.tbl_permit_workflow_next_step.Where(m => m.workflow_code == workflowCode);
                        _context.tbl_permit_workflow_next_step.RemoveRange(removeWorkflowNextStepEntity);
                    }

                    // 3. Save changes
                    _context.SaveChanges();

                    // 4. Add logging / historical data
                    LogUserActivity("WorkflowManagement", $"Workflow Deleted", $"{workflowName} workflow deleted", apkDateTime: DateTime.Now);

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
