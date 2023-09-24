using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class MonitoringController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
