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
                        //var ChainsawList = _context.tbl_chainsaw.ToList();

                        var ChainsawList = (from cs in _context.tbl_chainsaw
                                                    join b in _context.tbl_brands on cs.brand_id equals b.id into brandGroup
                                                    from brandTBL in brandGroup.DefaultIfEmpty()
                                                    select new tbl_chainsaw
                                                    {
                                                        Id = cs.Id,
                                                        user_id = cs.user_id,
                                                        tbl_application_id = cs.tbl_application_id,
                                                        brand_id = brandTBL.id,
                                                        Brand = brandTBL.name,
                                                        Model = cs.Model,
                                                        Engine = cs.Engine,
                                                        Power = cs.Power,
                                                        remarks = cs.remarks,
                                                        status = cs.status,
                                                        watt = cs.watt,
                                                        hp = cs.hp,
                                                        watt_dec = cs.watt_dec,
                                                        hp_dec = cs.hp_dec,
                                                        gb = cs.gb,
                                                        supplier = cs.supplier,
                                                        date_purchase = cs.date_purchase,
                                                        is_active = cs.is_active,
                                                        date_created = cs.date_created,
                                                        date_modified = cs.date_modified,
                                                        created_by = cs.created_by,
                                                        modified_by = cs.modified_by,
                                                        chainsaw_serial_number = cs.chainsaw_serial_number,
                                                        chainsaw_date_of_registration = cs.chainsaw_date_of_registration,
                                                        chainsaw_date_of_expiration = cs.chainsaw_date_of_expiration,
                                                        specification = cs.specification,
                                                        purpose = cs.purpose,
                                                    }).ToList();

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

                    //Add Chart Access Rights Here
                    var divisionOfficeList = _context.tbl_division.ToList();
                    mymodel.tbl_Division_List = divisionOfficeList;

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

        [HttpPost]
        public List<object> GetChartsData(string DateType, DateTime? startDate, DateTime? endDate, int? divisionID)
        {
            List<object> data = new List<object>();

            //Set endDate to end of the day (11:59pm)
            if (endDate.HasValue)
            {
                // Set the time component of endDate to end of the day (23:59:59)
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            //var uniqueSerialNumbersCount = _context.tbl_chainsaw
            //    .Select(c => c.chainsaw_serial_number)  // Select only the serial numbers
            //    .Distinct()                             // Get distinct serial numbers
            //    .Count();

            // Current date and time
            DateTime currentTime = DateTime.Now;// Convert.ToDateTime("2027-04-12 09:39:34.530");

            // LINQ query to categorize chainsaw serial numbers into New, Renew, and Expired categories
            var result = from chainsaw in _context.tbl_chainsaw
                         join application in _context.tbl_application on chainsaw.tbl_application_id equals application.id
                         join user in _context.tbl_user on application.tbl_user_id equals user.id //for location filters
                         let isRenew = application.renew_from != null && application.date_of_expiration >= currentTime
                         let isNew = application.date_of_expiration >= currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration > currentTime)
                         let isExpired = application.date_of_expiration < currentTime && !_context.tbl_application.Any(a => a.renew_from == application.id && a.date_of_expiration != null)
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
                             regionID = user.tbl_region_id,
                             provinceID = user.tbl_province_id,
                             cityID = user.tbl_city_id
                             //PlotDate = isRenew ? application.date_of_registration : (isNew ? application.date_of_registration : (isExpired ? application.date_of_expiration : (isPendingRenew ? null : (isPendingNew ? null : (isExpiredButRenewed ? null : (isRenewedButSoonToExpire ? application.date_of_registration : null))))))
                         };

            //Filter by Start Date and End Date
            if (startDate != null && endDate != null)
            {
                result = result.Where(r => (r.Category != "Expired" ? (r.DateRegistered >= startDate && r.DateRegistered <= endDate) : (r.DateExpired >= startDate && r.DateExpired <= endDate)));
            }

            //Filter by Office
            if (divisionID != null)
            {
                var divisionOffice = _context.tbl_division.Where(d => d.id == divisionID).FirstOrDefault();
                if (divisionOffice.city_id != 0 && divisionOffice.city_id != null)
                {
                    result = result.Where(r => r.cityID == divisionOffice.city_id);
                }
                else
                {
                    if (divisionOffice.province_id != 0 && divisionOffice.province_id != null)
                    {
                        result = result.Where(r => r.provinceID == divisionOffice.province_id);
                    }
                    else // (divisionOffice.region_id != 0 && divisionOffice.region_id != null) 
                    {
                        result = result.Where(r => r.regionID == divisionOffice.region_id);
                    }
                }
            }
            /*
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
            */




            //Filter Counts
            //if (startDate != null && endDate != null)
            //{
            //    groupedNewlyRegisteredDateCounts = groupedNewlyRegisteredDateCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedRenewedChainsawsDateCounts = groupedRenewedChainsawsDateCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedExpiredDateCounts = groupedExpiredDateCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();

            //    groupedNewlyRegisteredMonthCounts = groupedNewlyRegisteredMonthCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedRenewedChainsawsMonthCounts = groupedRenewedChainsawsMonthCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedExpiredMonthCounts = groupedExpiredMonthCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();

            //    groupedNewlyRegisteredYearCounts = groupedNewlyRegisteredYearCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedRenewedChainsawsYearCounts = groupedRenewedChainsawsYearCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //    groupedExpiredYearCounts = groupedExpiredYearCounts.Where(g => g.x >= startDate && g.x <= endDate).ToList();
            //}

            // Get minimum and maximum date on plot
            var resultNewAndRenewDates = result.Where(r => r.Category == "New" || r.Category == "Renewal").Select(r => r.DateRegistered).ToList();
            var resultExpiredDates = result.Where(r => r.Category == "Expired").Select(r => r.DateExpired).ToList();
            var allResultDates = resultNewAndRenewDates.Concat(resultExpiredDates).ToList();
            var minimumDate = allResultDates.Count() != 0 ? allResultDates.Min().Value.ToString("MMMM dd, yyyy") : null;
            var maximumDate = allResultDates.Count() != 0 ? allResultDates.Max().Value.ToString("MMMM dd, yyyy") : null;

            if (startDate != null  && endDate != null)
            {
                minimumDate = startDate.Value.ToString("MMMM dd, yyyy");
                maximumDate = endDate.Value.ToString("MMMM dd, yyyy");
            }

            if (DateType == "day")
            {
                //Day Counts
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
                .Where(r => r.Category == "Renewal")
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

                //// overall counts
                //var totalNewlyRegisteredCounts = result.Count(c => c.Category == "New");
                //var totalRenewedChainsawsCounts = result.Count(c => c.Category == "Renewal");
                //var totalExpiredDateCounts = result.Count(c => c.Category == "Expired");

                data.Add(groupedNewlyRegisteredDateCounts);
                data.Add(groupedRenewedChainsawsDateCounts);
                data.Add(groupedExpiredDateCounts);
                data.Add(minimumDate);
                data.Add(maximumDate);
            }
            else if (DateType == "month")
            {
                //Month Counts
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
                .Where(r => r.Category == "Renewal")
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
                .GroupBy(entry => new DateTime(entry.DateExpired.Value.Year, entry.DateExpired.Value.Month, 1)) // Group by month (ignoring day and time)
                .Select(group => new
                {
                    x = Convert.ToDateTime(group.Key.ToString("MMMM 01 yyyy")), // Format month as "Month Year" (e.g., "April 2024")
                    y = group.Count() // Count of expired registrations in this month
                })
                .ToList();

                data.Add(groupedNewlyRegisteredMonthCounts.OrderBy(g => g.x).ToList());
                data.Add(groupedRenewedChainsawsMonthCounts.OrderBy(g => g.x).ToList());
                data.Add(groupedExpiredMonthCounts.OrderBy(g => g.x).ToList());
                data.Add(minimumDate);
                data.Add(maximumDate);
            }
            else// if (selectedTimeCategory == "year")
            {
                //Year Counts
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
                .Where(r => r.Category == "Renewal")
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

                data.Add(groupedNewlyRegisteredYearCounts);
                data.Add(groupedRenewedChainsawsYearCounts);
                data.Add(groupedExpiredYearCounts);
                data.Add(minimumDate);
                data.Add(maximumDate);
            }
            return data;
        }

    }
}
