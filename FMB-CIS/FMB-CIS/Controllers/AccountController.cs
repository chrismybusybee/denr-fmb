﻿using FMB_CIS.Models;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web.Helpers;
using System.Net;
using reCAPTCHA.AspNetCore;
using Newtonsoft;


namespace FMB_CIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LocalContext _context;
       
        private IEmailSender EmailSender { get; set; }

        public AccountController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
           
        }


        public IActionResult Registration()
        {
            UserRegistrationViewModel model = new UserRegistrationViewModel();


            model.reCaptcha = new RECaptcha();

            var _regions = _context.tbl_region.ToList();
            var _provinces = new List<tbl_province>();
            var _cities = new List<tbl_city>();
            var _barangays = new List<tbl_brgy>();


            _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
            _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
            _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });
            _barangays.Add(new tbl_brgy() { id = 0, name = "-- Select Barangay --" });

            ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");
            ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.id), "id", "name");
            ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.id), "id", "name");
            ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.id), "id", "name");

            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;

            //return View();

            return View(model);
        }

        [HttpPost, ActionName("GetProvinceByRegionId")]
        public JsonResult GetProvinceByRegionId(string tbl_region_id)
        {
            int regID;
            List<tbl_province> provinceLists = new List<tbl_province>();
            if (!string.IsNullOrEmpty(tbl_region_id))
            {
                regID = Convert.ToInt32(tbl_region_id);
                if (regID == 13)
                { 
                    provinceLists = _context.tbl_province.Where(s => s.regCode.Equals(regID)).ToList(); 
                }
                else
                {
                    provinceLists = _context.tbl_province.Where(s => s.regCode.Equals(regID)).OrderBy(s => s.name).ToList();
                }    
            }
            return Json(provinceLists);
        }

        [HttpPost, ActionName("GetCityByProvinceId")]
        public JsonResult GetCityByProvinceId(string tbl_province_id)
        {
            int provID;
            List<tbl_city> cityLists = new List<tbl_city>();
            if (!string.IsNullOrEmpty(tbl_province_id))
            {
                provID = Convert.ToInt32(tbl_province_id);
                cityLists = _context.tbl_city.Where(s => s.provCode.Equals(provID)).OrderBy(s => s.name).ToList();
            }
            return Json(cityLists);
        }

        [HttpPost, ActionName("GetBrgyByCityId")]
        public JsonResult GetBrgyByCityId(string tbl_city_id)
        {
            int ctID;
            List<tbl_brgy> brgyLists = new List<tbl_brgy>();
            if (!string.IsNullOrEmpty(tbl_city_id))
            {
                ctID = Convert.ToInt32(tbl_city_id);
                brgyLists = _context.tbl_brgy.Where(s => s.citymunCode.Equals(ctID)).OrderBy(s => s.name).ToList();
            }
            return Json(brgyLists);
        }

        //[HttpPost]
        //public JsonResult AjaxMethod(string response)
        //{
        //    RECaptcha recaptcha = new RECaptcha();
        //    string url = "https://www.google.com/recaptcha/api/siteverify?secret=" + recaptcha.Secret + "&response=" + response;
        //    recaptcha.Response = (new WebClient()).DownloadString(url);
        //    return Json(recaptcha);
        //}
        public IActionResult RegistrationPrimary()
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
        public async Task<IActionResult> Registration(UserRegistrationViewModel model)
        {
            //[Bind("first_name,middle_name,last_name,suffix,contact_no,valid_id,valid_id_no,birth_date,tbl_region_id,tbl_province_id,tbl_city_id,tbl_brgy_id,street_address,tbl_division_id,email,password,confirmPassword,comment,tbl_user_types_id")]

            if (ModelState.IsValid)
            {
                DAL dal = new DAL();
                bool eMailExist = dal.emailExist(model.tbl_Users.email, _configuration.GetConnectionString("ConnStrng"));
                
                if (eMailExist == true)
                {
                    ModelState.AddModelError("tbl_Users.email", "Email Address has already been used, please try a different Email Address.");

                    return Registration();
                    //return View(model);
                }
                else
                {

                    //Generate Temporary Password
                    string temPass = TempPass.Generate(32, 15);
                    //ENCRYPT TEMPORARY PASSWORD
                    string encrPw = EncryptDecrypt.ConvertToEncrypt(temPass);
                    int one = 1;
                    
                    //SET VALUES FOR VARIABLES WITHOUT INPUT FIELDS ON VIEW
                    //tbl_division_id = model.tbl_division_id
                    model.tbl_Users.password = encrPw;
                    model.tbl_Users.status = false;
                    //model.tbl_Users.comment;
                    model.tbl_Users.date_created = DateTime.Now;
                    model.tbl_Users.date_modified = DateTime.Now;
                    //model.tbl_Users.tbl_user_types_id = Convert.ToInt32(model.tbl_Users.tbl_user_types_id);

                    //Save Info to Database
                    _context.tbl_user.Add(model.tbl_Users);
                    _context.SaveChanges();
                    
                    int? usrID = model.tbl_Users.id;

                    //File Upload
                    if (model.filesUpload != null)
                    {
                        foreach (var file in model.filesUpload.Files)
                        {
                            var filesDB = new tbl_files();
                            FileInfo fileInfo = new FileInfo(file.FileName);
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "Files/UserDocs");

                            //create folder if not exist
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);


                            string fileNameWithPath = Path.Combine(path, file.FileName);

                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            filesDB.tbl_user_id = usrID;
                            filesDB.created_by = (int)usrID;
                            filesDB.modified_by = (int)usrID;
                            filesDB.date_created = DateTime.Now;
                            filesDB.date_modified = DateTime.Now;
                            filesDB.filename = file.FileName;
                            filesDB.path = path;
                            filesDB.tbl_file_type_id = fileInfo.Extension;
                            filesDB.tbl_file_sources_id = fileInfo.Extension;
                            _context.tbl_files.Add(filesDB);
                            _context.SaveChanges();
                        }
                    }


                    //Code to set password for newly registered user
                    using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                        {
                            SqlCommand cmd = new SqlCommand("spResetPassword", sqlConnection);
                            cmd.CommandType = CommandType.StoredProcedure;
                            SqlParameter paramEmail = new SqlParameter("@email", model.tbl_Users.email);
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
                                    EmailSender.SendEmailAsync(model.tbl_Users.email, subject, body);
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

            return Registration();
            //return View(model);
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
                    }

                }
                else
                {
                    return View();
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
