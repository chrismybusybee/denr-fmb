using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class AnnouncementTemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
