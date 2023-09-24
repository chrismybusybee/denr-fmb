using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class EmailTemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
