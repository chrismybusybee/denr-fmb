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
using System.Security.Cryptography;
using Services.Utilities;


namespace FMB_CIS.Controllers
{
   
    public class DashboardController : Controller
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
        public DashboardController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewModel mymodel = new ViewModel();
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string userRole = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;

            //CHECK IF USER STATUS IS APPROVED OR NOT
            bool? userStatus = _context.tbl_user.Where(u => u.id == userID).Select(u => u.status).SingleOrDefault();

            if (userStatus == false)
            {
                LogUserActivity("Login", "Login", "Logged in but account is not approved yet", apkDateTime: DateTime.Now);
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            else
            {


                LogUserActivity("Login", "Login", "Logged in to system", apkDateTime: DateTime.Now);

                if (((ClaimsIdentity)User.Identity).FindFirst("userID").Value != null)
                {    

                    var dashboardView = new DashboardView();
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                    var myAccessRights = ((ClaimsIdentity)User.Identity).FindFirst("accessRights").Value; //Access Rights of the User

                    List<int> pendingIds = new List<int>(){
                        1, 3, 4, 6, 7, 9, 12, 14, 16, 18
                    };
                    List<int> declinedIds = new List<int>(){
                        2, 5, 8, 10, 13, 15, 17, 19
                    };
                    List<int> approvedIds = new List<int>(){
                        11
                    };

                    dashboardView.UserActiveCount = _context.tbl_user.Count();
                    dashboardView.UserChangeRequestCount =  _context.tbl_user_change_info_request.Count();
                    dashboardView.OfficeTypesCount = _context.tbl_office_type.Where(a => a.is_active == true).Count();
                    dashboardView.OfficesCount = _context.tbl_office.Where(a => a.is_active == true).Count();
                    dashboardView.UserTypesCount = _context.tbl_user_types.Where(a => a.is_active == true).Count();
                    dashboardView.AccessRightsCount = _context.tbl_access_right.Where(a => a.is_active == true).Count();
                    dashboardView.WorkflowsCount = _context.tbl_permit_workflow.Where(a => a.is_active == true).Count();
                    dashboardView.AnnouncementsCount = _context.tbl_announcement.Where(a => a.is_active == true).Count();
                    dashboardView.EmailsCount = _context.tbl_email_template.Where(a => a.is_active == true).Count();
                    dashboardView.ChecklistsCount = _context.tbl_document_checklist.Where(a => a.is_active == true).Count();

                    //Count for Chainsaw Applicants
                    dashboardView.SellerPendingCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                        pendingIds.Contains((int)a.status)).Count();
                    dashboardView.SellerApprovedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                        approvedIds.Contains((int)a.status)).Count();
                    dashboardView.SellerDeclinedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                        declinedIds.Contains((int)a.status)).Count();
                    dashboardView.SellerTotalCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER).Count();

                    dashboardView.OwnerPendingCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                        pendingIds.Contains((int)a.status)
                    ).Count();
                    dashboardView.OwnerApprovedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                        approvedIds.Contains((int)a.status)).Count();
                    dashboardView.OwnerDeclinedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                        declinedIds.Contains((int)a.status)).Count();
                    dashboardView.OwnerTotalCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER).Count();

                    dashboardView.ImporterPendingCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                        pendingIds.Contains((int)a.status)).Count();
                    dashboardView.ImporterApprovedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                        approvedIds.Contains((int)a.status)).Count();
                    dashboardView.ImporterDeclinedCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                        declinedIds.Contains((int)a.status)).Count();
                    dashboardView.ImporterTotalCount = _context.tbl_application.Where(a => a.is_active == true &&
                        a.tbl_user_id == userID &&
                        a.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER).Count();


                    //Count for DENR Side
                    //Regions are also filtered

                    if (AccessRightsUtilities.IsAccessRights(myAccessRights, "allow_page_manage_approve_reject_applications"))
                    {
                        //Get my region id
                        var myRegionID = _context.tbl_user.Where(u => u.id == loggedUserID).Select(u => u.tbl_region_id).FirstOrDefault();
                        
                        dashboardView.officerViewSellerPendingCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                            pendingIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewSellerApprovedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                            approvedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewSellerDeclinedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                            declinedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));


                        dashboardView.officerViewSellerTotalCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_SELLER &&
                            (joinResult.User.tbl_region_id == myRegionID));


                        dashboardView.officerViewOwnerPendingCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                            pendingIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewOwnerApprovedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                            approvedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewOwnerDeclinedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                            declinedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewOwnerTotalCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_OWNER &&
                            (joinResult.User.tbl_region_id == myRegionID));


                        dashboardView.officerViewImporterPendingCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                            pendingIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewImporterApprovedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                            approvedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewImporterDeclinedCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                            declinedIds.Contains((int)joinResult.Application.status) &&
                            (joinResult.User.tbl_region_id == myRegionID));

                        dashboardView.officerViewImporterTotalCount = _context.tbl_application
                        .Join(_context.tbl_user,
                            app => app.tbl_user_id,
                            user => user.id,
                            (app, user) => new { Application = app, User = user })
                        .Count(joinResult =>
                            joinResult.Application.is_active == true &&
                            joinResult.Application.tbl_application_type_id == (int)ApplicationTypeEnum.CHAINSAW_IMPORTER &&
                            (joinResult.User.tbl_region_id == myRegionID));
                    }

                    mymodel.dashboardView = dashboardView;



                    if(userRole.Contains("Owner") == true || userRole.Contains("Seller") == true || userRole.Contains("Importer") == true)
                    {

                    //OWNED CHAINSAWS
                    var ChainsawList = _context.tbl_chainsaw.ToList();
                    var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == userID /*&& m.status == "Seller"*/).ToList();

                        //HISTORY
                        var applicationlist = from a in _context.tbl_application
                                              where a.tbl_user_id == userID
                                              select a;

                    
                        var applicationtypelist = _context.tbl_application_type;

                        var applicationMod = from a in applicationlist
                                             join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                             join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                             join pS in _context.tbl_permit_status on a.status equals pS.id
                                             where a.tbl_user_id == userID
                                             select new ApplicationModel { id = a.id, application_type = appt.name, permit_type = pT.name, permit_status = pS.status };
                        mymodel.tbl_Chainsaws = ChainsawOwnedList;
                        mymodel.applicationModels = applicationMod;


                        return View(mymodel);
                    }
                    else if(userRole.Contains("CENRO") == true)
                    {
                        var applicationlist = _context.tbl_application;
                        var applicationtypelist = _context.tbl_application_type;

                    var applicationMod = from a in applicationlist
                                         join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                         join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                         join pS in _context.tbl_permit_status on a.status equals pS.id
                                         where a.tbl_user_id == userID
                                         select new ApplicationModel{id = a.id, application_type = appt.name, permit_type = pT.name, permit_status = pS.status};
                    //mymodel.tbl_Chainsaws = ChainsawOwnedList;
                    mymodel.applicationModels = applicationMod;

                        return View(mymodel);
                    }
                    else if(userRole.Contains("CENRO") == true)
                    {
                        var applicationlist = _context.tbl_application;
                        var applicationtypelist = _context.tbl_application_type;

                        var applicationMod = from a in applicationlist
                                             join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                             join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                             join pS in _context.tbl_permit_status on a.status equals pS.id
                                             select new ApplicationModel { 
                                                 id = a.id, 
                                                 application_type = appt.name, 
                                                 permit_type = pT.name, 
                                                 permit_status = pS.status, 
                                                 FullName = a.supplier_fname + " " + a.supplier_mname + " " + a.supplier_lname + " " + a.supplier_suffix,
                                                 Email = a.supplier_email
                                             };
                        mymodel.applicationModels = applicationMod;

                        return View(mymodel);
                    }                
                }
            }
            return View(mymodel);
            
        }

        public IActionResult UserHistory()
        {
            var ChainsawList = _context.tbl_chainsaw.ToList();
            int user_id = 1;
            var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == user_id).ToList();


            //return ChainsawOwnedList;
            return View();
        }


    }
}
