using FMB_CIS.Data;
using FMB_CIS.Models;
using Mapster;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        public ActivityLogsController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }
        [RequiresAccess(allowedAccessRights = "allow_page_activity_logs")]
        public IActionResult Index()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                ViewModel model = new ViewModel();
                //Get the list of activity logs

                var activityLogsList = (from ua in _context.tbl_user_activitylog
                                        join user in _context.tbl_user on ua.UserId equals user.id
                                        orderby ua.id descending
                                        select new tbl_user_activitylog
                                        {
                                            id = ua.id,
                                            email = user.email,
                                            UserId = ua.UserId,
                                            Entity = ua.Entity,
                                            UserAction = ua.UserAction,
                                            Remarks = ua.Remarks,
                                            CreatedDt = ua.CreatedDt,
                                            CreatedTimestamp = ua.CreatedTimestamp,
                                            ApkDatetime = ua.ApkDatetime,
                                            Source = ua.Source
                                        });
                model.tbl_Activity_logs = activityLogsList;

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
