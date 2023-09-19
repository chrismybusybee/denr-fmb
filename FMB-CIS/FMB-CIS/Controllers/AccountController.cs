using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using FMB_CIS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        //private readonly ILogger<AccountController> _logger;
        //private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IConfiguration configuration/*, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger*/)
        {
            //_logger = logger;
            //_userManager = userManager;
            this._configuration = configuration;
        }
        public class ApplicationUser : IdentityUser
        {
        }
        //public class ApplicationUser : IdentityUser
        //{
        //    //public virtual string user { get; set; }
        //}
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
        public IActionResult Registration([Bind("first_name,middle_name,last_name,suffix,contact_no,valid_id,valid_id_no,birth_date,tbl_region_id,tbl_province_id,tbl_city_id,tbl_brgy_id,street_address,tbl_division_id,email,password,confirmPassword,comment,tbl_user_types_id")] UserRegistrationViewModel userRegistrationViewModel)
        {
            if (ModelState.IsValid)
            {
                if (userRegistrationViewModel.confirmPassword == userRegistrationViewModel.password)
                { 
                    DAL dal = new DAL();
                    bool eMailExist = dal.emailExist(userRegistrationViewModel.email, _configuration.GetConnectionString("ConnStrng"));

                    if (eMailExist == true)
                    {
                        ModelState.AddModelError("email", "Email Address has already been used, please try a different Email Address.");
                        return View(userRegistrationViewModel);
                    }
                    else
                    {
                        //ENCRYPT PASSWORD
                        string encrPw = EncryptDecrypt.ConvertToEncrypt(userRegistrationViewModel.password);
                        //SAVE USER INFOS ON DATABASE USING CreateNewUserNoPHOTO STORED PROCEDURE
                        int one = 1;

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
                            sqlCmd.Parameters.AddWithValue("tbl_division_id", userRegistrationViewModel.tbl_division_id ?? "");
                            sqlCmd.Parameters.AddWithValue("email", userRegistrationViewModel.email);
                            sqlCmd.Parameters.AddWithValue("password", encrPw);
                            sqlCmd.Parameters.AddWithValue("status", one/*userRegistrationViewModel.status*/);
                            //sqlCmd.Parameters.AddWithValue("photo", userRegistrationViewModel.photo);
                            sqlCmd.Parameters.AddWithValue("comment", userRegistrationViewModel.comment ?? "");
                            sqlCmd.Parameters.AddWithValue("date_created", DateTime.Now);
                            sqlCmd.Parameters.AddWithValue("date_modified", DateTime.Now);
                            sqlCmd.Parameters.AddWithValue("tbl_user_types_id", Convert.ToInt32(userRegistrationViewModel.tbl_user_types_id));
                            sqlCmd.ExecuteNonQuery();
                        }
                        return RedirectToAction("EmailConfirmation");
                    }
                }
                else
                {
                    ModelState.AddModelError("confirmPassword", "Password doesn't match! Please enter again.");
                    return View(userRegistrationViewModel);
                }
            }
            return View(userRegistrationViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                //var user = await _userManager.FindByEmailAsync(model.email);
                //if (user != null /*&& await userManager.IsEmailConfirmedAsync(user)*/)
                //{
                //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                //    var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = model.email, token = token }, Request.Scheme);

                //    _logger.Log(LogLevel.Warning, passwordResetLink);
                //    return View("EmailConfirmation");
                //}
                //return View(model);
                DAL dal = new DAL();
                bool eMailExist = dal.emailExist(model.email, _configuration.GetConnectionString("ConnStrng"));
                if (eMailExist)
                {
                    //CODE TO GENERATE A LINK FOR PASSWORD RESET.

                    //var token = GeneratePasswordResetTokenAsync(model.email);

                    return RedirectToAction("EmailConfirmation");
                }
                else
                {                    
                    //Do not reveal if email doesn't exist.
                    return RedirectToAction("EmailConfirmation");
                }
            }
            
            return View(model);
        }

        //public IActionResult EnterNewPassword()
        //{
        //    return View();

        //}

        //[HttpPost]
        //public IActionResult EnterNewPassword(ForgotPasswordModel model, string email)
        //{
        //    if (model.confirmPassword == model.password)
        //    {
        //        string encrPw = EncryptDecrypt.ConvertToEncrypt(model.password);
        //        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
        //        {
        //            sqlConnection.Open();
        //            SqlCommand sqlCmd = new SqlCommand("ChangePasswordFromEmail", sqlConnection);
        //            sqlCmd.CommandType = CommandType.StoredProcedure;
        //            sqlCmd.Parameters.AddWithValue("email", email);
        //            sqlCmd.Parameters.AddWithValue("password", encrPw);
        //            sqlCmd.ExecuteNonQuery();
        //        }
        //        return View();
        //}

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
