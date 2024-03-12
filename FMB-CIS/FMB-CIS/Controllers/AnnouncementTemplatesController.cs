using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class AnnouncementTemplatesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LocalContext _context;
        private IEmailSender EmailSender { get; set; }

        public AnnouncementTemplatesController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;

        }
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

        public IActionResult Index()
        {
            string roleOfLoggedUser = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
            int usrTypeID = _context.tbl_user_types.Where(utype => utype.name == roleOfLoggedUser).Select(utype => utype.id).FirstOrDefault();
            
            if (usrTypeID == 13 || usrTypeID == 14)
            {
                //ONLY THE FOLLOWING ROLES CAN CHANGE TEMPLATES
                //13 - DENR CIS Administrator,
                //14 - DENR CIS Super Admin,
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                ViewModel model = new ViewModel();
                //var userinfoList                    

                var tblAnnouncementTemplateList = _context.tbl_announcement.ToList();
                model.tbl_Announcement_List = tblAnnouncementTemplateList;

                model.announcementViewModelList = (from annc in _context.tbl_announcement
                                                   join atype in _context.tbl_announcement_type on annc.tbl_announcement_type_id equals atype.id
                                                   select new AnnouncementViewModel
                                                   {
                                                       tbl_announcement_id = annc.id,
                                                       title = annc.title,
                                                       tbl_announcement_type_id = annc.tbl_announcement_type_id,
                                                       tbl_announcement_type_name = atype.name,
                                                       announcement_subject = annc.announcement_subject,
                                                       announcement_content = annc.announcement_content,
                                                       date_publish = annc.date_publish,
                                                       date_expiry = annc.date_expiry,
                                                       date_created = annc.date_created
                                                   });

                //For List of Announcements in modal dropdown
                var tblAnnouncementTypeList = _context.tbl_announcement_type.ToList();
                model.tbl_Announcement_Type_List = tblAnnouncementTypeList;

                return View(model);
                
            }
            else
            {
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            //return View();
        }

        [HttpPost]
        public IActionResult CreateEditAnnouncementTemplate(ViewModel model, string typeOfAction)
        {
            //USED FOR BOTH CREATE AND EDIT
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            if (typeOfAction == "create")
            {
                var announcement = new tbl_announcement();
                //emailTemplate.id
                announcement.title = model.soloAnnouncement.title;
                announcement.tbl_announcement_type_id = model.soloAnnouncement.tbl_announcement_type_id;
                announcement.announcement_subject = model.soloAnnouncement.announcement_subject;
                announcement.announcement_content = model.soloAnnouncement.announcement_content;
                announcement.date_publish = DateTime.Now;
                announcement.date_expiry = DateTime.Now;
                announcement.is_active = true;
                announcement.created_by = loggedUserID;
                announcement.modified_by = loggedUserID;
                announcement.date_created = DateTime.Now;
                announcement.date_published = DateTime.Now;
                _context.Add(announcement);
                _context.SaveChanges();

                var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                LogUserActivity("CreateAnnouncementTemplate", "Create Announcement Template", $"{announcement.title} Announcement Template created", apkDateTime: DateTime.Now);

            }
            else if (typeOfAction == "edit")
            {
                var announcementDB = _context.tbl_announcement.Where(e => e.id == model.soloAnnouncement.id).FirstOrDefault();

                announcementDB.title = model.soloAnnouncement.title;
                announcementDB.tbl_announcement_type_id = model.soloAnnouncement.tbl_announcement_type_id;
                announcementDB.announcement_subject = model.soloAnnouncement.announcement_subject;
                announcementDB.announcement_content = model.soloAnnouncement.announcement_content;
                //announcementDB.date_publish = DateTime.Now;
                announcementDB.date_expiry = DateTime.Now;
                //announcementDB.is_active = true;
                //announcementDB.created_by = loggedUserID;
                announcementDB.modified_by = loggedUserID;
                //announcementDB.date_created = DateTime.Now;
                announcementDB.date_published = DateTime.Now;
                               
                _context.Update(announcementDB);
                _context.SaveChanges();

                var userEmail = ((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value;
                LogUserActivity("UpdateAnnouncementTemplate", "Update Announcement Template", $"{announcementDB.title} Announcement Template updated", apkDateTime: DateTime.Now);

            }
            return RedirectToAction("Index", "AnnouncementTemplates");
        }

        [HttpPost, ActionName("GetAnnouncementTemplateDetails")]
        public JsonResult GetAnnouncementTemplateDetails(int id)
        {
            var announcementDetais = _context.tbl_announcement.Where(e => e.id == id).FirstOrDefault();
            return Json(announcementDetais);

        }

    }
}
