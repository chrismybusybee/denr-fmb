using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using FMB_CIS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Extensions;

namespace FMB_CIS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this._configuration = configuration;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard"); ;
            }
            //return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(LoginViewModel credentials)
        {
            //bool isLoggedIn = new bool();
            if (ModelState.IsValid)
            {
                //CHECK IF THE USER EXIST
                DAL dal = new DAL();
                bool eMailExist = dal.emailExist(credentials.email, _configuration.GetConnectionString("ConnStrng"));

                if (eMailExist == false)
                {
                    ModelState.AddModelError("email", "Email or Password is incorrect.");
                    return View();

                }

                else
                {
                    string decrPw = EncryptDecrypt.ConvertToDecrypt(dal.selectEncrPassFromEmail(credentials.email, _configuration.GetConnectionString("ConnStrng")));
                    //CHECK IF PASSWORD MATCH
                    if (credentials.password != decrPw)
                    {
                        ModelState.AddModelError("password", "Email or Password is incorrect.");
                        return View();
                    }

                    else
                    {

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, credentials.email),
                            new Claim("EmailAdd", credentials.email),
                            new Claim("FullName", dal.selectFullNameFromEmail(credentials.email, _configuration.GetConnectionString("ConnStrng"))),
                            new Claim("userID", dal.selectUserIDFromEmail(credentials.email, _configuration.GetConnectionString("ConnStrng"))),
                            new Claim(ClaimTypes.Role, dal.selectUserRoleFromEmail(credentials.email, _configuration.GetConnectionString("ConnStrng"))),
                            new Claim("userRole", dal.selectUserRoleFromEmail(credentials.email, _configuration.GetConnectionString("ConnStrng"))),
                            //new Claim(ClaimTypes.Role, "Administrator"),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            // Refreshing the authentication session should be allowed.

                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                            // The time at which the authentication ticket expires. A 
                            // value set here overrides the ExpireTimeSpan option of 
                            // CookieAuthenticationOptions set with AddCookie.

                            IsPersistent = true,
                            // Whether the authentication session is persisted across 
                            // multiple requests. When used with cookies, controls
                            // whether the cookie's lifetime is absolute (matching the
                            // lifetime of the authentication ticket) or session-based.

                            //IssuedUtc = <DateTimeOffset>,
                            // The time at which the authentication ticket was issued.

                            //RedirectUri = <string>
                            // The full path or absolute URI to be used as an http 
                            // redirect response value.
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        _logger.LogInformation("User {Email} logged in at {Time}.", credentials.email, DateTime.UtcNow);
                        //isLoggedIn = true;
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }
            return View();
        }

        //public async Task Signout(/*string returnUrl = null*/)
        //{
        //    //if (!string.IsNullOrEmpty(ErrorMessage))
        //    //{
        //    //    ModelState.AddModelError(string.Empty, ErrorMessage);
        //    //}

        //    // Clear the existing external cookie
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    return View();
        //}
        public ActionResult Signout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index");
        }

        public IActionResult TermsPolicy()
        {
            return View();
        }
    }
}