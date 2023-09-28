using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using FMB_CIS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using NuGet.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FMB_CIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

        public AccountController(IConfiguration configuration, IEmailSender emailSender)
        {
            this._configuration = configuration;
            EmailSender = emailSender;
        }


        //public async Task<IActionResult> Send(string toAddress)
        //{
        //    var subject = "sample subject";
        //    var body = "sample body";
        //    await EmailSender.SendEmailAsync(toAddress, subject, body);
        //    return View();
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

        //public IActionResult ResetPasswordSent()
        //{
        //    return View();
        //}

        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration([Bind("first_name,middle_name,last_name,suffix,contact_no,valid_id,valid_id_no,birth_date,tbl_region_id,tbl_province_name,tbl_city_name,tbl_brgy_name,street_address,tbl_division_id,email,password,confirmPassword,comment,tbl_user_types_id")] UserRegistrationViewModel userRegistrationViewModel)
        {
            if (ModelState.IsValid)
            {
                //if (userRegistrationViewModel.confirmPassword == userRegistrationViewModel.password)
                //{
                    DAL dal = new DAL();
                    bool eMailExist = dal.emailExist(userRegistrationViewModel.email, _configuration.GetConnectionString("ConnStrng"));

                    if (eMailExist == true)
                    {
                        ModelState.AddModelError("email", "Email Address has already been used, please try a different Email Address.");
                        return View(userRegistrationViewModel);
                    }
                    else
                    {
                        //Generate Temporary Password
                        string temPass = TempPass.Generate(32, 15);
                        //ENCRYPT TEMPORARY PASSWORD
                        string encrPw = EncryptDecrypt.ConvertToEncrypt(temPass);
                        //SAVE USER INFOS ON DATABASE USING CreateNewUserNoPHOTO STORED PROCEDURE
                        int one = 1;

                        //DAL.RegisterNewUser(userRegistrationViewModel.first_name, userRegistrationViewModel.middle_name, userRegistrationViewModel.last_name, userRegistrationViewModel.suffix, userRegistrationViewModel.contact_no, userRegistrationViewModel.valid_id, userRegistrationViewModel.valid_id_no, userRegistrationViewModel.birth_date, userRegistrationViewModel.tbl_region_id, userRegistrationViewModel.tbl_province_id, userRegistrationViewModel.tbl_city_id, userRegistrationViewModel.tbl_brgy_id, userRegistrationViewModel.street_address, userRegistrationViewModel.tbl_division_id, userRegistrationViewModel.email, encrPw, userRegistrationViewModel.status, userRegistrationViewModel.comment);
                        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                        {
                            sqlConnection.Open();
                            SqlCommand sqlCmd = new SqlCommand("CreateNewUserNoPHOTOandNOTID", sqlConnection);
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
                        sqlCmd.Parameters.AddWithValue("tbl_province_name", userRegistrationViewModel.tbl_province_name);
                        sqlCmd.Parameters.AddWithValue("tbl_city_name", userRegistrationViewModel.tbl_city_name);
                        sqlCmd.Parameters.AddWithValue("tbl_brgy_name", userRegistrationViewModel.tbl_brgy_name);
                        //sqlCmd.Parameters.AddWithValue("tbl_province_id", userRegistrationViewModel.tbl_province_id);
                        //sqlCmd.Parameters.AddWithValue("tbl_city_id", userRegistrationViewModel.tbl_city_id);
                        //sqlCmd.Parameters.AddWithValue("tbl_brgy_id", userRegistrationViewModel.tbl_brgy_id);
                        sqlCmd.Parameters.AddWithValue("street_address", userRegistrationViewModel.street_address);
                            sqlCmd.Parameters.AddWithValue("tbl_division_id", userRegistrationViewModel.tbl_division_id ?? "");
                            sqlCmd.Parameters.AddWithValue("email", userRegistrationViewModel.email);
                            sqlCmd.Parameters.AddWithValue("password", encrPw);
                            sqlCmd.Parameters.AddWithValue("status", one/*userRegistrationViewModel.status*/);
                            //sqlCmd.Parameters.AddWithValue("photo", userRegistrationViewModel.photo);
                            sqlCmd.Parameters.AddWithValue("comment", userRegistrationViewModel.comment ?? " ");
                            sqlCmd.Parameters.AddWithValue("date_created", DateTime.Now);
                            sqlCmd.Parameters.AddWithValue("date_modified", DateTime.Now);
                            sqlCmd.Parameters.AddWithValue("tbl_user_types_id", Convert.ToInt32(userRegistrationViewModel.tbl_user_types_id));
                            sqlCmd.ExecuteNonQuery();

                            sqlConnection.Close();

                        }
                        //Code to set password for newly registered user
                        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                        {
                            SqlCommand cmd = new SqlCommand("spResetPassword", sqlConnection);
                            cmd.CommandType = CommandType.StoredProcedure;
                            SqlParameter paramEmail = new SqlParameter("@email", userRegistrationViewModel.email);
                            cmd.Parameters.Add(paramEmail);

                            sqlConnection.Open();
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                if (Convert.ToBoolean(rdr["ReturnCode"]))
                                {
                                    string passResetLink = "https://fmb-cis.beesuite.ph/Account/ResetPassword?email=" + rdr["email"].ToString() + "&tokencode=" + rdr["UniqueId"].ToString();
                                
                                    Console.WriteLine("Link for Password Reset:");
                                    Console.WriteLine(passResetLink);
                                    var subject = "Account Created";
                                    var body = "We would like to inform you that you have created an account with FMB-CIS.\nPlease change your password on this link: " + passResetLink;
                                    EmailSender.SendEmailAsync(userRegistrationViewModel.email, subject, body);
                                    return RedirectToAction("EmailConfirmation");
                                }
                                else
                                {
                                    //Do not reveal if email doesn't exist.
                                    return RedirectToAction("EmailConfirmation");
                                }
                            }
                        }

                    //var subject = "Account has been created";
                    //var body = "We would like to inform you that you have created an account with FMB-CIS.\nIf you forgot your password or if you wish to change it, you may proceed on this link: https://fmb-cis.beesuite.ph/Account/ForgotPassword";
                    //EmailSender.SendEmailAsync(userRegistrationViewModel.email, subject, body);
                    return RedirectToAction("EmailConfirmation");

                    }
                //}
                //else
                //{
                //    ModelState.AddModelError("confirmPassword", "Password doesn't match! Please enter again.");
                //    return View(userRegistrationViewModel);
                //}
            }
            return View(userRegistrationViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                DAL dal = new DAL();
                bool eMailExist = dal.emailExist(model.email, _configuration.GetConnectionString("ConnStrng"));
                if (eMailExist)
                {
                    //CODE TO GENERATE A LINK FOR PASSWORD RESET.
                    using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                    {
                        SqlCommand cmd = new SqlCommand("spResetPassword", sqlConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter paramEmail = new SqlParameter("@email", model.email);
                        cmd.Parameters.Add(paramEmail);

                        sqlConnection.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (Convert.ToBoolean(rdr["ReturnCode"]))
                            {
                                string passResetLink = "https://fmb-cis.beesuite.ph/Account/ResetPassword?email=" + rdr["email"].ToString() + "&tokencode=" + rdr["UniqueId"].ToString();
                                //string passResetLink = "https://localhost:7270/Account/ResetPassword?email=" + rdr["email"].ToString() + "&tokencode=" + rdr["UniqueId"].ToString();

                                //SendPasswordResetEmail(rdr["Email"].ToString(), txtUserName.Text, rdr["UniqueId"].ToString());
                                //lblMessage.Text = "An email with instructions to reset your password is sent to your registered email";

                                //Write to text file
                                //Pass the filepath and filename to the StreamWriter Constructor

                                //StreamWriter sw = new StreamWriter("C:\\Users\\John Anthony\\Desktop\\Test.txt");

                                //Write a line of text
                                //sw.WriteLine("Email: " + rdr["email"].ToString());

                                //Write a second line of text
                                //sw.WriteLine("UniqueId: " + rdr["UniqueId"].ToString());

                                //sw.WriteLine(passResetLink);
                                //Close the file
                                //sw.Close();
                                Console.WriteLine("Link for Password Reset:");
                                Console.WriteLine(passResetLink);
                                var subject = "Password Reset";
                                var body = "Your Password Reset Link is: " + passResetLink;
                                await EmailSender.SendEmailAsync(model.email, subject, body);
                                return RedirectToAction("EmailConfirmation");
                            }
                            else
                            {
                                //Do not reveal if email doesn't exist.
                                return RedirectToAction("EmailConfirmation");
                            }
                        }
                    }

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

        [HttpGet]
        //[Url("?email={email}&code={code}")]
        public IActionResult ResetPassword(string email, string tokencode)
        {
            if (tokencode == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string cstr = _configuration.GetConnectionString("ConnStrng");
                //Check if token is valid
                DAL dal = new DAL();
                bool linkValidCheck = dal.isLinkValid(model.tokencode, cstr);
                bool eMailExist = dal.emailExist(model.email, cstr);
                if (linkValidCheck == true && eMailExist == true)
                {
                    string encrPw = EncryptDecrypt.ConvertToEncrypt(model.Password);
                    //Code to change password
                    bool isPWchanged = dal.changePasswordAndReturnIfChanged(model.tokencode, encrPw, cstr);
                    if (isPWchanged == true)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return View();
                        //return RedirectToAction("Index", "Home");
                    }

                    //return View("ResetPasswordConfirmation");
                }
                else
                {
                    return View();
                    //return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                //return RedirectToAction("Index", "Home");
                return View();
            }
        }


    }
}
