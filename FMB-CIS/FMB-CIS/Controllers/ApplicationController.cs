using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class ApplicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
