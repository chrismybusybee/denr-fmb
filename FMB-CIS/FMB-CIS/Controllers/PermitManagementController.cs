using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Ajax.Utilities;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class PermitManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

        public PermitManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        [HttpGet, ActionName("GetPermitTypes")]
        public JsonResult GetPermitTypes()
        {
            List<tbl_permit_types> permitTypes = new List<tbl_permit_types>();
            permitTypes = _context.tbl_permit_types.OrderBy(d => d.id).ToList();
            return Json(permitTypes);
        }

        [HttpGet, ActionName("GetStatusCodes")]
        public JsonResult GetStatusCodes()
        {
            List<tbl_permit_statuses> statusCodes = new List<tbl_permit_statuses>();
            statusCodes = _context.tbl_permit_statuses.OrderBy(d => d.id).ToList();
            return Json(statusCodes);
        }
    }
}
