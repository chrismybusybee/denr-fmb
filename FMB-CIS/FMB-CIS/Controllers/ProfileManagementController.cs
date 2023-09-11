using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class ProfileManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
