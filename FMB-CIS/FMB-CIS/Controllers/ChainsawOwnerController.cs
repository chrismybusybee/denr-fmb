<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
<<<<<<< HEAD
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
using System.Runtime.ConstrainedExecution;
<<<<<<< HEAD
=======
=======

>>>>>>> updated the chainsaw owner contrller and view.
﻿using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
<<<<<<< HEAD
>>>>>>> Created a chainsaw owner controller
=======

>>>>>>> updated the chainsaw owner contrller and view.
=======
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
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
=======
>>>>>>> updated approval for permits applications

namespace FMB_CIS.Controllers
{
    public class ChainsawOwnerController : Controller
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD

=======
    {
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
=======
    {
>>>>>>> Created a chainsaw owner controller
=======
    {
<<<<<<< HEAD

>>>>>>> updated the chainsaw owner contrller and view.
=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;

        public ChainsawOwnerController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
        }
<<<<<<< HEAD
<<<<<<< HEAD



        public IActionResult Index()
        {
<<<<<<< HEAD
<<<<<<< HEAD
            
=======
            List<PermitList> permitList = new List<PermitList>{
                new PermitList { Text = "Certificate of registration", Value = 13 },
                new PermitList { Text = "Permit to Re-sell/Transfer Ownership", Value = 14 }
                };

            ViewBag.permits = new SelectList(permitList, "ID", "Text");
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.

            return View();

        public IActionResult ChainsawOwnerApproval(int id)
        {
            ViewModel mymodel = new();
            var applicationlist = _context.tbl_application;

            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where a.id == id
                                 select new ApplicationModel
                                 {
                                     id = a.id,
                                     application_type = appt.name,
                                     permit_type = pT.name,
                                     permit_status = pS.status,
                                     FullName = a.supplier_fname + " " + a.supplier_mname + " " + a.supplier_lname + " " + a.supplier_suffix,
                                     Email = a.supplier_email
                                 };
            mymodel.applicationModels = applicationMod;

            var Application = _context.tbl_application;
            var ApplicationList = Application.Where(m => m.id == id).ToList();
=======
            
>>>>>>> updated approval for permits applications

            var UserDetails = from a in ApplicationList
                              join user in _context.tbl_user on a.tbl_user_id equals user.id
                                 where a.id == id
                                 select new tbl_user
                                 {
                                    FullName = user.first_name + " " + user.middle_name + " " + user.last_name + " " + user.suffix,
                                     tbl_user_types_id = user.tbl_user_types_id,
                                     email = user.email,
                                     valid_id = user.valid_id,
                                     valid_id_no = user.valid_id_no,
                                     contact_no = user.contact_no,
                                     birth_date = user.birth_date
                                 };
            mymodel.tbl_Users = UserDetails;
            return View(mymodel);
        }

<<<<<<< HEAD
        [HttpPost]
        public ActionResult Submit(tbl_application ta)
        {
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ta.tbl_application_type_id = 1;
            ta.tbl_user_id = userID;
            ta.is_active = true;
            ta.modified_by = userID;
            ta.created_by = userID;
            ta.status = 1;


            _context.tbl_application.Add(ta);
=======
=======

>>>>>>> updated the chainsaw owner contrller and view.
        public IActionResult Index()
        {
            return View();
        }
<<<<<<< HEAD
=======

>>>>>>> updated the chainsaw owner contrller and view.

<<<<<<< HEAD
=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
=======
        public IActionResult ChainsawOwnerApproval(int id)
        {
            ViewModel mymodel = new();
            var applicationlist = _context.tbl_application;

            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where a.id == id
                                 select new ApplicationModel
                                 {
                                     id = a.id,
                                     application_type = appt.name,
                                     permit_type = pT.name,
                                     permit_status = pS.status,
                                     FullName = a.supplier_fname + " " + a.supplier_mname + " " + a.supplier_lname + " " + a.supplier_suffix,
                                     Email = a.supplier_email
                                 };
            mymodel.applicationModels = applicationMod;

            var Application = _context.tbl_application;
            var ApplicationList = Application.Where(m => m.id == id).ToList();

            var UserDetails = from a in ApplicationList
                              join user in _context.tbl_user on a.tbl_user_id equals user.id
                                 where a.id == id
                                 select new tbl_user
                                 {
                                    FullName = user.first_name + " " + user.middle_name + " " + user.last_name + " " + user.suffix,
                                     tbl_user_types_id = user.tbl_user_types_id,
                                     email = user.email,
                                     valid_id = user.valid_id,
                                     valid_id_no = user.valid_id_no,
                                     contact_no = user.contact_no,
                                     birth_date = user.birth_date
                                 };
            mymodel.tbl_Users = UserDetails;
            return View(mymodel);
        }

>>>>>>> updated approval for permits applications
        [HttpPost]
        public ActionResult Submit(tbl_application ta)
        {
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
<<<<<<< HEAD
            sm.tbl_application_type_id = 3;
            sm.tbl_user_id = userID;
            sm.is_active = 1;
            sm.modified_by = userID;
            sm.created_by = userID;
            sm.status = 1;
            sm.tbl_permit_type_id = 3;

            _context.tbl_application.Add(sm);
>>>>>>> Created a chainsaw owner controller
=======
            ta.tbl_application_type_id = 1;
            ta.tbl_user_id = userID;
            ta.is_active = true;
            ta.modified_by = userID;
            ta.created_by = userID;
            ta.status = 1;


            _context.tbl_application.Add(ta);
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
            _context.SaveChanges();

            ViewBag.Status = "Save Success.";

            return View("Index");
        }
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> updated approval for permits applications
        [HttpPost]
        public ActionResult FormApprove(IFormCollection ta)
        {
            string comments = ta["item.application.others"];
            int id = Convert.ToInt32(ta["item.id"]);
            var application = _context.tbl_application.First(a => a.id == id);
            application.others = comments;
            application.status = 2;
            _context.SaveChanges();

            ViewBag.Status = "Approval Success.";

            return View();
        }

<<<<<<< HEAD
=======
>>>>>>> Created a chainsaw owner controller
=======

>>>>>>> updated the chainsaw owner contrller and view.
=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
=======
>>>>>>> updated approval for permits applications
    }
}
