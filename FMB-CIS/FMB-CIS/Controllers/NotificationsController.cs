using FMB_CIS.Data;
using FMB_CIS.Models;
using FMB_CIS.Models.Notifications;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment EnvironmentWebHost;

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
        public NotificationsController(IConfiguration configuration, LocalContext context, IEmailSender emailSender, IWebHostEnvironment _environment)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            EnvironmentWebHost = _environment;
        }

        [Authorize]
        [RequiresAccess(allowedAccessRights = "allow_page_notifications")]
        [HttpGet]
        public IActionResult Index()
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int loggedUserRegionId = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("regionID").Value);
            //int loggedUserRegionId = _context.tbl_user.Where(u=>u.id == loggedUserID).Select(u=>u.tbl_region_id).FirstOrDefault();
            DateTime currentDateAndTimeNow = DateTime.Now;

            var myUserTypes = _context.tbl_user_type_user
                .Where(u => u.user_id == loggedUserID && u.is_active == true)
                .Select(u => u.user_type_id)
                .ToList();

            //var combinedNotifs = _context.tbl_notifications
            //    .Where(n => (n.notified_user_id == loggedUserID && n.is_active && currentDateAndTimeNow >= n.date_notified)
            //                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_active && currentDateAndTimeNow >= n.date_notified))
            //    .OrderByDescending(n => n.date_notified)
            //    .ToList();
            var combinedNotifs = _context.tbl_notifications
                .Where(n => (n.notified_user_id == loggedUserID && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_region_exclusive == true && n.region_id_filter == loggedUserRegionId && n.is_active && currentDateAndTimeNow >= n.date_notified)
                || (n.tbl_notification_type_id == 3 && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true))
                .OrderByDescending(n => n.date_notified)
                .GroupJoin(
                _context.tbl_notification_read
                .Where(nr => nr.tbl_user_id == loggedUserID),
                n => n.id,
                nr => nr.tbl_notifications_id,
                (notification, notificationReads) => new userNotificationsWithRead
                {
                    id = notification.id,
                    source_user_id = notification.source_user_id,
                    tbl_notification_type_id = notification.tbl_notification_type_id,
                    notified_user_id = notification.notified_user_id,
                    notified_user_type = notification.notified_user_type,
                    notification_title = notification.notification_title,
                    notification_content = notification.notification_content,
                    date_notified = notification.date_notified,
                    is_active = notification.is_active,
                    created_by = notification.created_by,
                    modified_by = notification.modified_by,
                    date_created = notification.date_created,
                    date_modified = notification.date_modified,
                    is_about_permit = notification.is_about_permit,
                    is_read = notificationReads.Any() ? (bool?)notificationReads.First().is_read : false
                }).ToList();

            var model = new NotificationsViewModel();
            model.userNotifications = combinedNotifs;
            //model.tbl_Notifications = combinedNotifs;
            return View(model);
        }

        [Authorize]
        [RequiresAccess(allowedAccessRights = "allow_page_notifications")]
        [HttpGet]
        public IActionResult GetRecentNotificationsForBell()
        {
            //int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ////int myUserType = _context.tbl_user_type_user.Where(u => u.user_id == loggedUserID && u.is_active == true).Select(u=>u.user_type_id).FirstOrDefault();
            //var myUserTypes = _context.tbl_user_type_user.Where(u => u.user_id == loggedUserID && u.is_active == true).Select(u => u.user_type_id).ToList();
            //var myNotifs = _context.tbl_notifications.Where(n => n.notified_user_id == loggedUserID && n.is_active == true && DateTime.Now >= n.date_notified).ToList();
            ////var myUserTypeNotifs = _context.tbl_notifications.Where(n=>n.notified_user_type == myUserType).ToList();
            ////var myUserTypeNotifs = _context.tbl_notifications.Where(n => n.notified_user_type == myUserTypes).ToList();
            //var myUserTypeNotifs = new List<tbl_notifications>();
            //var allUserTypeNotifs = new List<tbl_notifications>();
            //foreach (var item in myUserTypes)
            //{
            //    myUserTypeNotifs = _context.tbl_notifications.Where(n => n.notified_user_type == item && n.is_active == true && DateTime.Now >= n.date_notified).ToList();
            //    allUserTypeNotifs = allUserTypeNotifs.Concat(myUserTypeNotifs).ToList();
            //}



            //var model = new NotificationsViewModel();
            //model.tbl_Notifications = myNotifs.Concat(allUserTypeNotifs).OrderByDescending(o => o.date_notified).ToList();

            //int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            //var myUserTypes = _context.tbl_user_type_user
            //    .Where(u => u.user_id == loggedUserID && u.is_active == true)
            //    .Select(u => u.user_type_id)
            //    .ToList();

            //var myNotifs = _context.tbl_notifications
            //    .Where(n => n.notified_user_id == loggedUserID && n.is_active && DateTime.Now >= n.date_notified)
            //    .ToList();

            //var allUserTypeNotifs = myUserTypes
            //    .SelectMany(item => _context.tbl_notifications
            //        .Where(n => n.notified_user_type == item && n.is_active && DateTime.Now >= n.date_notified)
            //    )
            //    .ToList();

            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int loggedUserRegionId = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("regionID").Value);

            DateTime currentDateAndTimeNow = DateTime.Now;

            var myUserTypes = _context.tbl_user_type_user
                .Where(u => u.user_id == loggedUserID && u.is_active == true)
                .Select(u => u.user_type_id)
                .ToList();

            var pageSize = 3; // Set the desired page size
            var pageNumber = 1; // Set the desired page number

            //var combinedNotifs = _context.tbl_notifications
            //    .Where(n => (n.notified_user_id == loggedUserID && n.is_active && currentDateAndTimeNow >= n.date_notified)
            //                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_active && currentDateAndTimeNow >= n.date_notified))
            //    .OrderByDescending(n => n.date_notified)
            //    .Skip((pageNumber - 1) * pageSize)
            //    .Take(pageSize)
            //    .ToList();

            var combinedNotifs = _context.tbl_notifications
                .Where(n => (n.notified_user_id == loggedUserID && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_region_exclusive == true && n.region_id_filter == loggedUserRegionId && n.is_active && currentDateAndTimeNow >= n.date_notified)
                || (n.tbl_notification_type_id == 3 && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true))
                .OrderByDescending(n => n.date_notified)
                .GroupJoin(
                _context.tbl_notification_read
                .Where(nr => nr.tbl_user_id == loggedUserID),
                n => n.id,
                nr => nr.tbl_notifications_id,
                (notification, notificationReads) => new userNotificationsWithRead
                {
                    id = notification.id,
                    source_user_id = notification.source_user_id,
                    tbl_notification_type_id = notification.tbl_notification_type_id,
                    notified_user_id = notification.notified_user_id,
                    notified_user_type = notification.notified_user_type,
                    notification_title = notification.notification_title,
                    notification_content = notification.notification_content,
                    date_notified = notification.date_notified,
                    is_active = notification.is_active,
                    created_by = notification.created_by,
                    modified_by = notification.modified_by,
                    date_created = notification.date_created,
                    date_modified = notification.date_modified,
                    is_about_permit = notification.is_about_permit,
                    is_read = notificationReads.Any() ? (bool?)notificationReads.First().is_read : false
                })
                .OrderByDescending(n => n.date_notified)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new NotificationsViewModel();
            //model.tbl_Notifications = combinedNotifs;
            model.userNotifications = combinedNotifs;
            //myNotifs.Concat(allUserTypeNotifs).OrderByDescending(o => o.date_notified).ToList();
            return PartialView("~/Views/Notifications/RecentNotifsPartialView.cshtml", model);

        }

        [HttpPost, ActionName("ReadNotif")]
        [RequiresAccess(allowedAccessRights = "allow_page_notifications")]
        public JsonResult ReadNotif(int notifID, bool isRead)
        {
            try
            {
                var tblNotifFromDB = _context.tbl_notifications.Any(n => n.id == notifID);
                if (tblNotifFromDB != false)
                {
                    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

                    bool notifReadExists = _context.tbl_notification_read.Any(n => n.tbl_notifications_id == notifID && n.tbl_user_id == loggedUserID);

                    if (notifReadExists == true)
                    {
                        var myReadNotif = _context.tbl_notification_read.FirstOrDefault(n => n.tbl_notifications_id == notifID && n.tbl_user_id == loggedUserID);
                        myReadNotif.is_read = isRead;
                        myReadNotif.is_active = true;
                        myReadNotif.modified_by = loggedUserID;
                        myReadNotif.date_modified = DateTime.Now;
                        _context.SaveChanges();
                    }
                    else
                    {
                        var createNotifRead = new tbl_notification_read();
                        createNotifRead.tbl_notifications_id = notifID;
                        createNotifRead.tbl_user_id = loggedUserID;
                        createNotifRead.is_read = isRead;
                        createNotifRead.is_active = true;
                        createNotifRead.created_by = loggedUserID;
                        createNotifRead.modified_by = loggedUserID;
                        createNotifRead.date_created = DateTime.Now;
                        createNotifRead.date_modified = DateTime.Now;
                        _context.tbl_notification_read.Add(createNotifRead);
                        _context.SaveChanges();
                    }

                    return Json(new { success = true, is_Read = isRead });
                }
                else
                {
                    return Json(new { error = "Notification not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred while deleting the notification." });
            }
        }

        [HttpGet, ActionName("UnreadNotifCounter")]
        [RequiresAccess(allowedAccessRights = "allow_page_notifications")]
        public JsonResult UnreadNotifCounter()
        {
            try
            {
                int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                int loggedUserRegionId = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("regionID").Value);
                DateTime currentDateAndTimeNow = DateTime.Now;

                var myUserTypes = _context.tbl_user_type_user
                    .Where(u => u.user_id == loggedUserID && u.is_active == true)
                    .Select(u => u.user_type_id)
                    .ToList();

                var countUnreadNotifs = _context.tbl_notifications
                .Where(n => (n.notified_user_id == loggedUserID && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true)
                || (myUserTypes.Contains((int)n.notified_user_type) && n.is_region_exclusive == true && n.region_id_filter == loggedUserRegionId && n.is_active && currentDateAndTimeNow >= n.date_notified)
                || (n.tbl_notification_type_id == 3 && n.is_active && currentDateAndTimeNow >= n.date_notified && n.is_region_exclusive != true))
                .OrderByDescending(n => n.date_notified)
                .GroupJoin(
                _context.tbl_notification_read
                .Where(nr => nr.tbl_user_id == loggedUserID),
                n => n.id,
                nr => nr.tbl_notifications_id,
                (notification, notificationReads) => new userNotificationsWithRead
                {
                    id = notification.id,
                    source_user_id = notification.source_user_id,
                    tbl_notification_type_id = notification.tbl_notification_type_id,
                    notified_user_id = notification.notified_user_id,
                    notified_user_type = notification.notified_user_type,
                    notification_title = notification.notification_title,
                    notification_content = notification.notification_content,
                    date_notified = notification.date_notified,
                    is_active = notification.is_active,
                    created_by = notification.created_by,
                    modified_by = notification.modified_by,
                    date_created = notification.date_created,
                    date_modified = notification.date_modified,
                    is_about_permit = notification.is_about_permit,
                    is_read = notificationReads.Any() ? (bool?)notificationReads.First().is_read : false
                }).Count(n => n.is_read == false);

                return Json(new { success = true, unreadNotifsCount = countUnreadNotifs });
                
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred while deleting the notification." });
            }
        }

        //FOR MANAGE NOTIFICATIONS
        [Authorize]
        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpGet]
        public IActionResult ManageNotifications()
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //var myUserTypes = _context.tbl_user_type_user.Where(u => u.user_id == loggedUserID && u.is_active == true).Select(u => u.user_type_id).ToList();

            var notifType = _context.tbl_notification_type.Where(nt => nt.is_active == true).ToList();
            var model = new ManageNotificationsViewModel();
            model.tbl_Notification_Types = notifType;
            //model.tbl_Notifications = allNotifs;

            var availableUserTypes = _context.tbl_user_types.Where(u => u.is_active == true).ToList();
            var usersList = _context.tbl_user.Where(u => u.is_active == true).ToList();
            model.tbl_Users = usersList;
            model.tbl_User_Types = availableUserTypes;
            return View(model);
        }

        [Authorize]
        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpGet]
        public IActionResult GetNotificationsFromType(int notifTypeID)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();

            ManageNotificationsViewModel model = new ManageNotificationsViewModel();

            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;

            //var allNotifs = _context.tbl_notifications.Where(n=>n.tbl_notification_type_id==notifTypeID).ToList();

            // Left outer join between tbl_notifications and tbl_user on notified_user_id
            var allNotifs = (from notification in _context.tbl_notifications
                             join user in _context.tbl_user on notification.notified_user_id equals user.id into userJoin
                             from user in userJoin.DefaultIfEmpty()

                                 // Left outer join between tbl_notifications and tbl_user_types on notified_user_type_id
                             join userType in _context.tbl_user_types on notification.notified_user_type equals userType.id into userTypeJoin
                             from userType in userTypeJoin.DefaultIfEmpty()
                             where notification.tbl_notification_type_id == notifTypeID
                             select new NotificationsJoinedValues
                             {
                                 tbl_notifications_id = notification.id,
                                 source_user_id = notification.source_user_id,
                                 tbl_notification_type_id = notification.tbl_notification_type_id,
                                 notified_user_id = notification.notified_user_id,
                                 notified_user_type = notification.notified_user_type,
                                 notification_title = notification.notification_title,
                                 notification_content = notification.notification_content,
                                 date_notified = notification.date_notified,
                                 is_active = notification.is_active,
                                 created_by = notification.created_by,
                                 modified_by = notification.modified_by,
                                 date_created = notification.date_created,
                                 date_modified = notification.date_modified,//joined
                                                                            //tbl_notification_type_name,
                                 notified_user_name = user.first_name + " " + user.middle_name + " " + user.last_name + " " + user.suffix,
                                 notified_user_type_name = userType.name
                             }).ToList();


            //model.tbl_Notifications = allNotifs;
            model.notificationsJoinedValues = allNotifs;
            return PartialView("~/Views/Notifications/ManageNotifPartialView.cshtml", model);

        }


        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpPost, ActionName("EnableDisableNotification")]
        public JsonResult EnableDisableNotification(int notifID, bool enableOrDisable)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var tblNotifFromDB = _context.tbl_notifications.Where(n => n.id == notifID).FirstOrDefault();

            tblNotifFromDB.is_active = enableOrDisable;
            tblNotifFromDB.date_modified = DateTime.Now;
            tblNotifFromDB.modified_by = uid;
            _context.SaveChanges();
            LogUserActivity("ManageNotifications", $"Notification {(enableOrDisable == true? "Enabled" : "Disabled")}", $"\"{tblNotifFromDB.notification_title}\" notification has been {(enableOrDisable == true ? "enabled" : "disabled")}", apkDateTime: DateTime.Now);
            return Json(true);
        }

        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpGet]
        public IActionResult GetNotificationInfo(int notifID)
        {
            try
            {
                // Code to fetch and process data
                int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                var tblNotifFromDB = _context.tbl_notifications.Where(n => n.id == notifID).FirstOrDefault();
                return Json(tblNotifFromDB);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error response
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        //Creates New Notification
        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpPost]
        public IActionResult CreateNotification(int? userID, int? userTypeID, int notifTypeID, string title, string content, DateTime dateNotified)
        {
            try
            {
                int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                var createNotif = new tbl_notifications();
                createNotif.source_user_id = null;
                createNotif.tbl_notification_type_id = notifTypeID;
                createNotif.notified_user_id = userID;
                createNotif.notified_user_type = userTypeID;
                createNotif.notification_title = title;
                createNotif.notification_content = content;
                createNotif.date_notified = dateNotified;
                createNotif.is_active = true;
                createNotif.created_by = loggedUserID;
                createNotif.modified_by = loggedUserID;
                createNotif.date_created = DateTime.Now;
                createNotif.date_modified = DateTime.Now;
                createNotif.is_about_permit = false; //Notifications about permit are not created here
                _context.tbl_notifications.Add(createNotif);
                _context.SaveChanges();
                LogUserActivity("ManageNotifications", "Notification Created", $"\"{createNotif.notification_title}\" notification was created", apkDateTime: DateTime.Now);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return Json(new { success = false });
            }
        }

        // Update/Edit a Notification
        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        [HttpPost]
        public IActionResult EditNotification(int? notificationID, string title, string content, DateTime dateNotified)
        {
            try
            {
                int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                var notifFromDB = _context.tbl_notifications.Where(n => n.id == notificationID).FirstOrDefault();
                notifFromDB.notification_title = title;
                notifFromDB.notification_content = content;
                notifFromDB.date_notified = dateNotified;
                notifFromDB.modified_by = loggedUserID;
                notifFromDB.date_modified = DateTime.Now;
                _context.SaveChanges();
                LogUserActivity("ManageNotifications", "Notification Updated", $"\"{notifFromDB.notification_title}\" notification has been updated", apkDateTime: DateTime.Now);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost, ActionName("DeleteNotif")]
        [RequiresAccess(allowedAccessRights = "allow_page_manage_notifications")]
        public JsonResult DeleteNotif(int notifID)
        {
            try
            {
                var tblNotifFromDB = _context.tbl_notifications.Find(notifID);
                if (tblNotifFromDB != null)
                {
                    _context.Remove(tblNotifFromDB);
                    _context.SaveChanges();

                    // Return more information about the deleted notification if needed
                    var deletedNotificationInfo = new
                    {
                        id = tblNotifFromDB.id,
                        title = tblNotifFromDB.notification_title,
                        content = tblNotifFromDB.notification_content,
                        // Add other properties as needed
                    };
                    LogUserActivity("ManageNotifications", "Notification Deleted", $"\"{tblNotifFromDB.notification_title}\" notification has been deleted", apkDateTime: DateTime.Now);
                    return Json(new { success = true, deletedNotification = deletedNotificationInfo });
                }
                else
                {
                    // Handle the case when the notification with the given ID is not found
                    return Json(new { error = "Notification not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way appropriate for your application
                return Json(new { error = "An error occurred while deleting the notification." });
            }
        }

        [HttpGet]
        public JsonResult GetNotificationTemplate()
        {
            var notificationTemplates = _context.tbl_announcement.Where(a => a.tbl_announcement_type_id == 2);
            return Json(notificationTemplates);

        }

    }

}
