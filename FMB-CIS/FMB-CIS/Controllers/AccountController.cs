using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using FMB_CIS.Data;

namespace FMB_CIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        
        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult EmailConfirmation()
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

        // POST
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration([Bind("first_name,middle_name,last_name,suffix,contact_no,valid_id,valid_id_no,birth_date,tbl_region_id,tbl_province_id,tbl_city_id,tbl_brgy_id,street_address,tbl_division_id,email,password,comment")] UserRegistrationViewModel userRegistrationViewModel)
        {
            if (ModelState.IsValid)
            {
                DAL dal = new DAL();
                bool eMailExist = dal.emailExist(userRegistrationViewModel.email, _configuration.GetConnectionString("ConnStrng"));

                if (eMailExist == true)
                {
                    ModelState.AddModelError("email", "Email Address has already been used, please try a different Email Address.");
                    return View();
                }
                else
                {
                    //ENCRYPT PASSWORD
                    string encrPw = EncryptDecrypt.ConvertToEncrypt(userRegistrationViewModel.password);
                    //SAVE USER INFOS ON DATABASE USING CreateNewUserNoPHOTO STORED PROCEDURE

                    //DAL.RegisterNewUser(userRegistrationViewModel.first_name, userRegistrationViewModel.middle_name, userRegistrationViewModel.last_name, userRegistrationViewModel.suffix, userRegistrationViewModel.contact_no, userRegistrationViewModel.valid_id, userRegistrationViewModel.valid_id_no, userRegistrationViewModel.birth_date, userRegistrationViewModel.tbl_region_id, userRegistrationViewModel.tbl_province_id, userRegistrationViewModel.tbl_city_id, userRegistrationViewModel.tbl_brgy_id, userRegistrationViewModel.street_address, userRegistrationViewModel.tbl_division_id, userRegistrationViewModel.email, encrPw, userRegistrationViewModel.status, userRegistrationViewModel.comment);
                    using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                    {
                        sqlConnection.Open();
                        SqlCommand sqlCmd = new SqlCommand("CreateNewUserNoPHOTO", sqlConnection);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("first_name", userRegistrationViewModel.first_name);
                        sqlCmd.Parameters.AddWithValue("middle_name", userRegistrationViewModel.middle_name);
                        sqlCmd.Parameters.AddWithValue("last_name", userRegistrationViewModel.last_name);
                        sqlCmd.Parameters.AddWithValue("suffix", userRegistrationViewModel.suffix ?? "");
                        sqlCmd.Parameters.AddWithValue("contact_no", userRegistrationViewModel.contact_no);
                        sqlCmd.Parameters.AddWithValue("valid_id", userRegistrationViewModel.valid_id);
                        sqlCmd.Parameters.AddWithValue("valid_id_no", userRegistrationViewModel.valid_id_no);
                        sqlCmd.Parameters.AddWithValue("birth_date", userRegistrationViewModel.birth_date);
                        sqlCmd.Parameters.AddWithValue("tbl_region_id", userRegistrationViewModel.tbl_region_id);
                        sqlCmd.Parameters.AddWithValue("tbl_province_id", userRegistrationViewModel.tbl_province_id);
                        sqlCmd.Parameters.AddWithValue("tbl_city_id", userRegistrationViewModel.tbl_city_id);
                        sqlCmd.Parameters.AddWithValue("tbl_brgy_id", userRegistrationViewModel.tbl_brgy_id);
                        sqlCmd.Parameters.AddWithValue("street_address", userRegistrationViewModel.street_address);
                        sqlCmd.Parameters.AddWithValue("tbl_division_id", userRegistrationViewModel.tbl_division_id);
                        sqlCmd.Parameters.AddWithValue("email", userRegistrationViewModel.email);
                        sqlCmd.Parameters.AddWithValue("password", encrPw);
                        sqlCmd.Parameters.AddWithValue("status", "active"/*userRegistrationViewModel.status*/);
                        //sqlCmd.Parameters.AddWithValue("photo", userRegistrationViewModel.photo);
                        sqlCmd.Parameters.AddWithValue("comment", userRegistrationViewModel.comment);
                        sqlCmd.Parameters.AddWithValue("date_created", DateTime.Now);
                        sqlCmd.Parameters.AddWithValue("date_modified", DateTime.Now);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("EmailConfirmation");
            }
            return View(userRegistrationViewModel);
        }
        
        //public static List<Regions> GetRegions()
        //{
        //    List<Regions> regionObj = new List<Regions>();
        //    string cstr = "Data Source=DESKTOP-LM30NHD\\MSSQLSERVER01;Database=denr_fmb_cis;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=true";
        //    using (SqlConnection sqlConnection = new SqlConnection(cstr))
        //    {
        //        using (SqlCommand sqlCmd = new SqlCommand("SelectAllRegionNames", sqlConnection))
        //        {
        //            using(SqlDataAdapter sDA = new SqlDataAdapter())
        //            {
        //                sqlCmd.Connection=sqlConnection;
        //                sqlConnection.Open();
        //                sDA.SelectCommand = sqlCmd;
        //                SqlDataReader sdr = SqlCommand.ExecuteReader();
        //                while (sdr.Read())
        //                {
        //                    Regions regObj = new Regions();
        //                    regObj.name = sdr["Name"].ToString();
        //                    regionObj.Add(regObj);
        //                }
        //            }
        //            return regionObj;
        //        }
        //    }
        //}

    }
}
