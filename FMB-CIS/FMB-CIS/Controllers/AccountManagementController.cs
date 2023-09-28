using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class AccountManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EditAccount()
        {
            return View();
        }
    }
}
