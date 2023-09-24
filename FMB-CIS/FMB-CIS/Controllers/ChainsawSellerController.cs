using System;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using FMB_CIS.Data;
using FMB_CIS.Models;

namespace FMB_CIS.Controllers
{
    public class ChainsawSellerController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;

        public ChainsawSellerController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Submit(tbl_application sm)
        {
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            sm.tbl_application_type_id = 3;
            sm.tbl_user_id = userID;
            sm.is_active = 1;
            sm.modified_by = userID;
            sm.created_by = userID;
            sm.status = 1;
            sm.tbl_permit_type_id = 3;

            _context.tbl_application.Add(sm);
            _context.SaveChanges();

            ViewBag.Status = "Save Success.";

            return View("Index");
        }
    }
}
