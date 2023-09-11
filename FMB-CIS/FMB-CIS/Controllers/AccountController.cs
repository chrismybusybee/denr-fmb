using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult ResetPasswordSent()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}
