using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class ReportGenerationController : Controller
    {
        [RequiresAccess(allowedAccessRights = "allow_page_report_generation")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
