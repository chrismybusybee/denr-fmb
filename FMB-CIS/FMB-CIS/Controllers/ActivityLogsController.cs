using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class ActivityLogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
