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
   
    public class DashboardController : Controller
    {

        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        
        public DashboardController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        [Authorize]
        
        public DashboardController(LocalContext context)
        {
            _context = context;
        }

        // GET: Movies
        public IActionResult Index()
        {
            ViewModel mymodel = new ViewModel();
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string userRole = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
             if (((ClaimsIdentity)User.Identity).FindFirst("userID").Value != null)
            {    
                /*DataTable dtbl = new DataTable();
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("CSawImporterRecordsForDashboard", sqlConnection);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    //sqlDa.SelectCommand.Parameters.AddWithValue("user_id", Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value));
                    sqlDa.SelectCommand.Parameters.AddWithValue("user_id", userID);
                    sqlDa.Fill(dtbl);
                }*/
                
            var ChainsawList = _context.tbl_chainsaw.ToList();
            var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == userID && m.status == userRole).ToList();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == user_id
                                  select a;


            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on appt.id equals pS.application_type
                                 where a.tbl_user_id == userID
                                 select new ApplicationModel{id = a.id, application_type = appt.name, permit_type = pT.name, permit_status = pS.status};
            mymodel.tbl_Chainsaws = ChainsawOwnedList;
            mymodel.applicationModels = applicationMod;

            return View(mymodel);

            }
            return View();
            
            
        }

        public IActionResult UserHistory()
        {
            var ChainsawList = _context.tbl_chainsaw.ToList();
            int user_id = 1;
            var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == user_id).ToList();

            return ChainsawOwnedList;
        }


    }
}
