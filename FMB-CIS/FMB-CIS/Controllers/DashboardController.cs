using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
