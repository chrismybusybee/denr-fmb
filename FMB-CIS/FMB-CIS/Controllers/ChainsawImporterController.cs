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
    public class ChainsawImporterController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;

        public ChainsawImporterController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult ChainsawImporterApproval(int id)
        {

            return View();
        }
    }
}
