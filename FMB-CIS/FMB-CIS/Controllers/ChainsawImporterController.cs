using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class ChainsawImporterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChainsawImporterApproval()
        {
            return View();
        }
    }
}
