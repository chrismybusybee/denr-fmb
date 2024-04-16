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
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

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
                //LogUserActivity("Login", "Login", "Logged in but account is not approved yet", apkDateTime: DateTime.Now);
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            else
            {


                //LogUserActivity("Login", "Login", "Logged in to system", apkDateTime: DateTime.Now);

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



                    //if(userRole.Contains("Owner") == true || userRole.Contains("Seller") == true || userRole.Contains("Importer") == true)
                    if (AccessRightsUtilities.IsAccessRights(((ClaimsIdentity)User.Identity).FindFirst("accessRights").Value, "allow_table_owned_chainsaws"))
                    {

                        //OWNED CHAINSAWS
                        var ChainsawList = _context.tbl_chainsaw.ToList();
                        var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == userID /*&& m.status == "Seller"*/).ToList();
                        mymodel.tbl_Chainsaws = ChainsawOwnedList;
                    }

                    if (AccessRightsUtilities.IsAccessRights(((ClaimsIdentity)User.Identity).FindFirst("accessRights").Value, "allow_table_history"))
                    {
                        //HISTORY
                        var applicationlist = from a in _context.tbl_application
                                              where a.tbl_user_id == userID
                                              select a;

                    
                        var applicationtypelist = _context.tbl_application_type;

                        var applicationMod = (from a in applicationlist
                                             join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                             join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                             join pS in _context.tbl_permit_status on a.status equals pS.id
                                             where a.tbl_user_id == userID
                                             select new ApplicationModel { 
                                                 id = a.id, 
                                                 application_type = appt.name, 
                                                 permit_type = pT.name, 
                                                 permit_status = pS.status,
                                                 application = a
                                             }).ToList();
                        
                        mymodel.applicationModels = applicationMod;

                    }
                    
                    ///*else*/ if(userRole.Contains("CENRO") == true)
                    //{
                    //    var applicationlist = _context.tbl_application;
                    //    var applicationtypelist = _context.tbl_application_type;

                    //var applicationMod = from a in applicationlist
                    //                     join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                    //                     join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                    //                     join pS in _context.tbl_permit_status on a.status equals pS.id
                    //                     where a.tbl_user_id == userID
                    //                     select new ApplicationModel{id = a.id, application_type = appt.name, permit_type = pT.name, permit_status = pS.status};
                    ////mymodel.tbl_Chainsaws = ChainsawOwnedList;
                    //mymodel.applicationModels = applicationMod;

                    //   // return View(mymodel);
                    //}
                    ///*else*/ if(userRole.Contains("CENRO") == true)
                    //{
                    //    var applicationlist = _context.tbl_application;
                    //    var applicationtypelist = _context.tbl_application_type;

                    //    var applicationMod = from a in applicationlist
                    //                         join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                    //                         join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                    //                         join pS in _context.tbl_permit_status on a.status equals pS.id
                    //                         select new ApplicationModel { 
                    //                             id = a.id, 
                    //                             application_type = appt.name, 
                    //                             permit_type = pT.name, 
                    //                             permit_status = pS.status, 
                    //                             FullName = a.supplier_fname + " " + a.supplier_mname + " " + a.supplier_lname + " " + a.supplier_suffix,
                    //                             Email = a.supplier_email
                    //                         };
                    //    mymodel.applicationModels = applicationMod;

                    //    //return View(mymodel);
                    //}
                    //Chainsaws for Sale
                    if (AccessRightsUtilities.IsAccessRights(((ClaimsIdentity)User.Identity).FindFirst("accessRights").Value, "allow_table_chainsaws_for_sale"))
                    {
                        var csawsForSale = (from ag in _context.tbl_application_group
                                                join b in _context.tbl_brands on ag.brand_id equals b.id into brandGroup
                                                from brandTBL in brandGroup.DefaultIfEmpty()
                                                join a in _context.tbl_application on ag.tbl_application_id equals a.id
                                                where a.tbl_user_id == loggedUserID && a.tbl_permit_type_id == 3
                                                select new tbl_application_group
                                                {
                                                    engine_serialNo = ag.engine_serialNo,
                                                    brand = brandTBL.name,
                                                    model = ag.model,
                                                    power_source = ag.power_source
                                                }).ToList();
                        mymodel.ChainsawsForSale = csawsForSale;
                    }
                    //Imported Chainsaws
                    if (AccessRightsUtilities.IsAccessRights(((ClaimsIdentity)User.Identity).FindFirst("accessRights").Value, "allow_table_imported_chainsaws"))
                    {
                        var importedCsaws = (from ag in _context.tbl_application_group
                                            join b in _context.tbl_brands on ag.brand_id equals b.id into brandGroup
                                            from brandTBL in brandGroup.DefaultIfEmpty()
                                            join a in _context.tbl_application on ag.tbl_application_id equals a.id
                                            where a.tbl_user_id == loggedUserID && a.tbl_permit_type_id == 1
                                            select new tbl_application_group
                                            {
                                                engine_serialNo = ag.engine_serialNo,
                                                brand = brandTBL.name,
                                                model = ag.model,
                                                power_source = ag.power_source
                                            }).ToList();
                        mymodel.ImportedChainsaws = importedCsaws;
                    }

                    ViewBag.Months = new SelectList(Enumerable.Range(1, 12).Select(x =>
                       new SelectListItem()
                       {
                           Text = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[x - 1] + " (" + x + ")",
                           Value = x.ToString()
                       }), "Value", "Text");



                    ViewBag.Years = new SelectList(Enumerable.Range(DateTime.Today.Year, 20).Select(x =>

                       new SelectListItem()
                       {
                           Text = x.ToString(),
                           Value = x.ToString()
                       }), "Value", "Text");

                    ViewBag.SelectedMonth = 3;


                    return View(mymodel);
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

        /*
        public List<object> GetChartsData()
        {
            List<object> data = new List<object>();
            //List<string> labels = _context.tbl_chainsaw.Select(x => x.chainsaw_date_of_expiration.ToString()).ToList();

            List<string> labels = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList();
            data.Add(labels);

            //List<string> applicationId = _context.tbl_chainsaw.Select(x => x.tbl_application_id.ToString()).ToList();   
            //data.Add(applicationId);

            //var uniqueSerialNumbersCount = _context.tbl_chainsaw
            //    .Select(c => c.chainsaw_serial_number)  // Select only the serial numbers
            //    .Distinct()                             // Get distinct serial numbers
            //    .Count();

            //List<tbl_chainsaw> tbl_chainsaws = new List<tbl_chainsaw>
            //{
            //    new tbl_chainsaw { Id = 1, tbl_application_id = 653, chainsaw_serial_number = "5SWDE6F1T32SDFV1" },
            //    new tbl_chainsaw { Id = 2, tbl_application_id = 654, chainsaw_serial_number = "5SWDE6F1T32SDFV1" },
            //    new tbl_chainsaw { Id = 3, tbl_application_id = 660, chainsaw_serial_number = "154EW7FFR1TG554DXC1F" },
            //    new tbl_chainsaw { Id = 4, tbl_application_id = 662, chainsaw_serial_number = "SN-1234" }
            //};

            //    List<tbl_application> tbl_applications = new List<tbl_application>
            //{
            //    new tbl_application { id = 653, date_of_registration = DateTime.Parse("2024-03-18 16:45:41.217"), date_of_expiration = DateTime.Parse("2024-03-18 16:45:41.217"), renew_from = null },
            //    new tbl_application { id = 654, date_of_registration = DateTime.Parse("2024-04-12 09:44:35.410"), date_of_expiration = DateTime.Parse("2027-04-12 09:44:35.410"), renew_from = 653 },
            //    new tbl_application { id = 660, date_of_registration = DateTime.Parse("2024-04-12 09:39:36.317"), date_of_expiration = DateTime.Parse("2027-04-12 09:39:36.317"), renew_from = null },
            //    new tbl_application { id = 662, date_of_registration = DateTime.Parse("2024-04-12 09:39:32.530"), date_of_expiration = DateTime.Parse("2027-04-12 09:39:32.530"), renew_from = null }
            //};

            // Current date and time
            DateTime currentTime = DateTime.Now;// Convert.ToDateTime("2027-04-12 09:39:34.530");

            // LINQ query to categorize chainsaw serial numbers into New, Renew, and Expired categories
            var result = from chainsaw in _context.tbl_chainsaw
                         join application in _context.tbl_application on chainsaw.tbl_application_id equals application.id
                         let isRenew = application.renew_from != null && application.date_of_expiration >= currentTime
                         let isNew = application.date_of_expiration >= currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration > currentTime)
                         let isExpired = application.date_of_expiration < currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration > currentTime)
                         let isPendingRenew = application.renew_from != null && application.date_of_registration == null
                         let isPendingNew = application.date_of_registration == null && !_context.tbl_application.Any(a => a.renew_from == application.id)
                         let isExpiredButRenewed = application.date_of_expiration < currentTime && _context.tbl_application.Any(a => a.renew_from == application.id)
                         let isRenewedButSoonToExpire = application.date_of_expiration > currentTime && _context.tbl_application.Any(a => a.renew_from == application.id)
                         //let yearRegistered = DateTime.Parse(application.date_of_registration.ToString()).Year
                         //let monthRegistered = DateTime.Parse(application.date_of_registration.ToString()).Month
                         //let yearExpired = DateTime.Parse(application.date_of_expiration.ToString()).Year
                         //let monthExpired = DateTime.Parse(application.date_of_expiration.ToString()).Month
                         select new
                         {
                             ChainsawSerialNumber = chainsaw.chainsaw_serial_number,
                             TblApplicationId = chainsaw.tbl_application_id,
                             Category = isRenew ? "Renewal" : (isNew ? "New" : (isExpired ? "Expired" : (isPendingRenew ? "Renewal-Pending" : (isPendingNew ? "New-Pending" : (isExpiredButRenewed ? "Expired-But-Renewed" : (isRenewedButSoonToExpire ? "Renewed-But-Soon-To-Expire" : "Unknown")))))),
                             DateRegistered = application.date_of_registration,
                             DateExpired = application.date_of_expiration,
                             //YearRegistered = DateTime.Parse(application.date_of_registration.ToString()).Year,
                             //MonthRegistered = DateTime.Parse(application.date_of_registration.ToString()).Month,
                             //YearExpired = DateTime.Parse(application.date_of_expiration.ToString()).Year,
                             //MonthExpired = DateTime.Parse(application.date_of_expiration.ToString()).Month
                         };


            // Grouping the results by category and creating a dictionary to store chainsaw data for each category
            var groupedResults = result.GroupBy(r => r.Category)
                                       .ToDictionary(
                                           g => g.Key,
                                           g => g.Select(r => new 
                                           { 
                                               ChainsawSerialNumber = r.ChainsawSerialNumber, 
                                               TblApplicationId = r.TblApplicationId, 
                                               DateRegistered = r.DateRegistered,
                                               DateExpired = r.DateExpired
                                           }).OrderBy(r=>r.TblApplicationId).ToList()
                                       );

            // Output chainsaw serial numbers categorized by each category
            foreach (var category in groupedResults.Keys)
            {
                Console.WriteLine($"{category} Category:");
                foreach (var serialNumber in groupedResults[category])
                {
                    Console.WriteLine($"- {serialNumber}");
                }
                Console.WriteLine();
            }

            var newlyRegistered = result.Where(r=>r.Category == "New").ToList();
            var renewedChainsaws = result.Where(r => r.Category == "Renewal").ToList();
            var expired = result.Where(r=> r.Category == "Expired").ToList();
            data.Add(newlyRegistered.Count());
            data.Add(renewedChainsaws.Count());
            data.Add(expired.Count());
            return data;
        }
        */

        public List<object> GetChartsData()
        {
            List<object> data = new List<object>();
            //List<string> labels = _context.tbl_chainsaw.Select(x => x.chainsaw_date_of_expiration.ToString()).ToList();

            //List<string> labels = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList();
            //data.Add(labels);

            var uniqueSerialNumbersCount = _context.tbl_chainsaw
                .Select(c => c.chainsaw_serial_number)  // Select only the serial numbers
                .Distinct()                             // Get distinct serial numbers
                .Count();

            // Current date and time
            DateTime currentTime = DateTime.Now;// Convert.ToDateTime("2027-04-12 09:39:34.530");

            // LINQ query to categorize chainsaw serial numbers into New, Renew, and Expired categories
            var result = from chainsaw in _context.tbl_chainsaw
                         join application in _context.tbl_application on chainsaw.tbl_application_id equals application.id
                         let isRenew = application.renew_from != null && application.date_of_expiration >= currentTime
                         let isNew = application.date_of_expiration >= currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration > currentTime)
                         let isExpired = application.date_of_expiration < currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration > currentTime)
                         let isPendingRenew = application.renew_from != null && application.date_of_registration == null
                         let isPendingNew = application.date_of_registration == null && !_context.tbl_application.Any(a => a.renew_from == application.id)
                         let isExpiredButRenewed = application.date_of_expiration < currentTime && _context.tbl_application.Any(a => a.renew_from == application.id)
                         let isRenewedButSoonToExpire = application.date_of_expiration > currentTime && _context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_registration != null)
                         select new
                         {
                             ChainsawSerialNumber = chainsaw.chainsaw_serial_number,
                             TblApplicationId = chainsaw.tbl_application_id,
                             Category = isRenew ? "Renewal" : (isNew ? "New" : (isExpired ? "Expired" : (isPendingRenew ? "Renewal-Pending" : (isPendingNew ? "New-Pending" : (isExpiredButRenewed ? "Expired-But-Renewed" : (isRenewedButSoonToExpire ? "Renewed-But-Soon-To-Expire" : "Unknown")))))),
                             DateRegistered = application.date_of_registration,
                             DateExpired = application.date_of_expiration,
                             //PlotDate = isRenew ? application.date_of_registration : (isNew ? application.date_of_registration : (isExpired ? application.date_of_expiration : (isPendingRenew ? null : (isPendingNew ? null : (isExpiredButRenewed ? null : (isRenewedButSoonToExpire ? application.date_of_registration : null))))))
                         };


            // Grouping the results by category and creating a dictionary to store chainsaw data for each category
            var groupedResults = result.GroupBy(r => r.Category)
                                       .ToDictionary(
                                           g => g.Key,
                                           g => g.Select(r => new
                                           {
                                               ChainsawSerialNumber = r.ChainsawSerialNumber,
                                               TblApplicationId = r.TblApplicationId,
                                               DateRegistered = r.DateRegistered,
                                               DateExpired = r.DateExpired
                                           }).OrderBy(r => r.TblApplicationId).ToList()
                                       );

            // Output chainsaw serial numbers categorized by each category
            foreach (var category in groupedResults.Keys)
            {
                Console.WriteLine($"{category} Category:");
                foreach (var serialNumber in groupedResults[category])
                {
                    Console.WriteLine($"- {serialNumber}");
                }
                Console.WriteLine();
            }

            //var newlyRegistered = result.Where(r => r.Category == "New").ToList();
            //var renewedChainsaws = result.Where(r => r.Category == "Renewal" || r.Category == "Renewed-But-Soon-To-Expire").ToList();
            //var expired = result.Where(r => r.Category == "Expired").ToList();

            //{ 
            var groupedNewlyRegisteredDateCounts = result
            .Where(r => r.Category == "New")
            .GroupBy(entry => entry.DateRegistered.Value.Date) // Group by date (ignoring time)
            .Select(group => new
            {
                x = group.Key, //Date
                y = group.Count() //Count
            })
            .ToList();

            var groupedRenewedChainsawsDateCounts = result
            .Where(r => r.Category == "Renewal" || r.Category == "Renewed-But-Soon-To-Expire")
            .GroupBy(entry => entry.DateRegistered.Value.Date) // Group by date (ignoring time)
            .Select(group => new
            {
                x = group.Key, //Date
                y = group.Count() //Count
            })
            .ToList();

            var groupedExpiredDateCounts = result
            .Where(r => r.Category == "Expired")
            .GroupBy(entry => entry.DateExpired.Value.Date) // Group by date (ignoring time)
            .Select(group => new
            {
                x = group.Key, //Date
                y = group.Count() //Count
            })
            .ToList();
            //}

            //{ 
            var groupedNewlyRegisteredMonthCounts = result
            .Where(r => r.Category == "New" && r.DateRegistered.HasValue) // Filter by category and non-null DateRegistered
            .GroupBy(entry => new DateTime(entry.DateRegistered.Value.Year, entry.DateRegistered.Value.Month, 1)) // Group by month (ignoring day and time)
            .Select(group => new
            {
                x = Convert.ToDateTime(group.Key.ToString("MMMM 01 yyyy")), // Format month as "Month Year" (e.g., "April 2024")
                y = group.Count() // Count of registrations in this month
            })
            .ToList();


            var groupedRenewedChainsawsMonthCounts = result
            .Where(r => r.Category == "Renewal" || r.Category == "Renewed-But-Soon-To-Expire")
            .Where(r => r.DateRegistered.HasValue) // Filter out entries with null DateRegistered
            .GroupBy(entry => new DateTime(entry.DateRegistered.Value.Year, entry.DateRegistered.Value.Month, 1)) // Group by month (ignoring day and time)
            .Select(group => new
            {
                x = Convert.ToDateTime(group.Key.ToString("MMMM 01 yyyy")), // Format month as "Month Year" (e.g., "April 2024")
                y = group.Count() // Count of renewals in this month
            })
            .ToList();


            var groupedExpiredMonthCounts = result
            .Where(r => r.Category == "Expired")
            .Where(r => r.DateRegistered.HasValue) // Filter out entries with null DateRegistered
            .GroupBy(entry => new DateTime(entry.DateRegistered.Value.Year, entry.DateRegistered.Value.Month, 1)) // Group by month (ignoring day and time)
            .Select(group => new
            {
                x = Convert.ToDateTime(group.Key.ToString("MMMM 01 yyyy")), // Format month as "Month Year" (e.g., "April 2024")
                y = group.Count() // Count of expired registrations in this month
            })
            .ToList();

            //}

            //{ 
            var groupedNewlyRegisteredYearCounts = result
            .Where(r => r.Category == "New")
            .GroupBy(entry => entry.DateRegistered.Value.Year) // Group by date (ignoring time)
            .Select(group => new
            {
                x = Convert.ToDateTime($"January 01 {group.Key}"), //Date
                y = group.Count() //Count
            })
            .ToList();

            var groupedRenewedChainsawsYearCounts = result
            .Where(r => r.Category == "Renewal" || r.Category == "Renewed-But-Soon-To-Expire")
            .GroupBy(entry => entry.DateRegistered.Value.Year) // Group by date (ignoring time)
            .Select(group => new
            {
                x = Convert.ToDateTime($"January 01 {group.Key}"), //Date
                y = group.Count() //Count
            })
            .ToList();

            var groupedExpiredYearCounts = result
            .Where(r => r.Category == "Expired")
            .GroupBy(entry => entry.DateExpired.Value.Year) // Group by date (ignoring time)
            .Select(group => new
            {
                x = Convert.ToDateTime($"January 01 {group.Key}"), //Date
                y = group.Count() //Count
            })
            .ToList();
            //}

            string selectedTimeCategory = "month";

            if(selectedTimeCategory == "day")
            {
                data.Add(groupedNewlyRegisteredDateCounts);
                data.Add(groupedRenewedChainsawsDateCounts);
                data.Add(groupedExpiredDateCounts);
            }
            else if (selectedTimeCategory == "month")
            {
                data.Add(groupedNewlyRegisteredMonthCounts);
                data.Add(groupedRenewedChainsawsMonthCounts);
                data.Add(groupedExpiredMonthCounts);
            }
            else// if (selectedTimeCategory == "year")
            {
                data.Add(groupedNewlyRegisteredYearCounts);
                data.Add(groupedRenewedChainsawsYearCounts);
                data.Add(groupedExpiredYearCounts);
            }
            return data;
        }

    }
}
