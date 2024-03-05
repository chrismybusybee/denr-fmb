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

        public IActionResult Index()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                ViewModel model = new ViewModel();
                //Get the list of activity logs
                var activityLogsList = _context.tbl_user_activitylog.OrderByDescending(u => u.id).ToList();
                model.tbl_Activity_logs = activityLogsList;

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public IActionResult ActivityLogsListPartialView()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 14) // Super Admin
            {
                ActivityLogsListViewModel model = new ActivityLogsListViewModel();
                //Get the list of users
                var entities = _context.tbl_user_activitylog.OrderByDescending(u=>u.CreatedDt).ToList();
                model.activityLogs = entities.Adapt<List<ActivityLog>>();
                
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                return PartialView("~/Views/ActivityLogs/Partial/ActivityLogsListPartial.cshtml", model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
