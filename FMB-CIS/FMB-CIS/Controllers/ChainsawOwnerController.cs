<<<<<<< HEAD
﻿using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
=======
﻿using Microsoft.AspNetCore.Mvc;
>>>>>>> e3daccb9689d065fc715a78e1e7e1dfe36b0ec15

namespace FMB_CIS.Controllers
{
    public class ChainsawOwnerController : Controller
    {
<<<<<<< HEAD
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;

        public ChainsawOwnerController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
        }
=======
>>>>>>> e3daccb9689d065fc715a78e1e7e1dfe36b0ec15
        public IActionResult Index()
        {
            return View();
        }
<<<<<<< HEAD

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
=======
>>>>>>> e3daccb9689d065fc715a78e1e7e1dfe36b0ec15
    }
}
