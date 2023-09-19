using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using FMB_CIS.Models;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        [Authorize]
        public IActionResult Index()
        {
            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            if (((ClaimsIdentity)User.Identity).FindFirst("userID").Value != null)
            {    
                DataTable dtbl = new DataTable();
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("CSawImporterRecordsForDashboard", sqlConnection);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    //sqlDa.SelectCommand.Parameters.AddWithValue("user_id", Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value));
                    sqlDa.SelectCommand.Parameters.AddWithValue("user_id", userID);
                    sqlDa.Fill(dtbl);
                }
                return View(dtbl);
            }
            return View();
        }


    }
}
