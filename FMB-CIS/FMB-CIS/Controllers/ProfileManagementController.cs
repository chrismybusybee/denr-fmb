using FMB_CIS.Data;
using FMB_CIS.Models;
using FMB_CIS.Models.ManageProfile;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class ProfileManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment EnvironmentWebHost;

        public void LogUserActivity(string entity, string userAction, string remarks, int userId = 0, string source = "Web", DateTime? apkDateTime = null)
        {
            try
            {
                if (entity.ToUpper() == "LOGOUT"
                    && source.ToUpper() == "WEB")
                {
                    var fullname = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("FullName").Value);
                    remarks = "User logged out. Username: " + fullname;
                }

                int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                //Inserting record to UserActivityLog database
                tbl_user_activitylog activityLog = new tbl_user_activitylog()
                {
                    UserId = uid,
                    Entity = entity,
                    UserAction = (userAction ?? ""),
                    Remarks = (remarks ?? ""),
                    CreatedDt = DateTime.Now.Date,
                    CreatedTimestamp = DateTime.Now,
                    ApkDatetime = apkDateTime,
                    Source = source
                };
                _context.Add(activityLog);
                _context.SaveChanges();

                ////Inserting record to UserActivity Log file
                //var userdata = _userRepository.TableNoTracking.Where(x => x.Id == (_userSession.UserId != 0 ? _userSession.UserId : userId))
                //              .Select(y => y.UserCode + " (" + y.FirstName + ")").FirstOrDefault();
                //Utility.Logger.UserLog("The " + userdata + " In Module " + entity + "-" + (userAction ?? "") + ":" + (remarks ?? ""));


            }
            catch (Exception ex)
            {
            }
        }
        public ProfileManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender, IWebHostEnvironment _environment)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            EnvironmentWebHost = _environment;
        }
        [RequiresAccess(allowedAccessRights = "allow_page_manage_profile")]
        public IActionResult Index()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var usInfo = _context.tbl_user.Where(u => u.id == uid).SingleOrDefault();
            ManageProfileViewModel model = new ManageProfileViewModel();


            var currentUser = _context.tbl_user.Find(uid);
            model.tbl_User = currentUser;

            //var _regions = _context.tbl_region.ToList();
            //var _provinces = _context.tbl_province.Where(p => p.regCode == usInfo.tbl_region_id).ToList();
            //var _cities = _context.tbl_city.Where(c => c.provCode == usInfo.tbl_province_id).ToList();
            //var _barangays = _context.tbl_brgy.Where(b => b.citymunCode == usInfo.tbl_city_id).ToList();

            //_regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
            //_provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
            //_cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });
            //_barangays.Add(new tbl_brgy() { id = 0, name = "-- Select Barangay --" });

            //ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");

            //ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.name), "id", "name");
            //ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.name), "id", "name");
            //ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.name), "id", "name");

            //string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            //ViewData["BaseUrl"] = host;           

            //User Types
            var myUserTypes = (from usrRoles in _context.tbl_user_type_user
                               where usrRoles.user_id == uid && usrRoles.is_active == true
                               join utypesName in _context.tbl_user_types on usrRoles.user_type_id equals utypesName.id
                               select new
                               {
                                   Roles = utypesName.name
                               }).ToList();
            ViewBag.UserTypes = myUserTypes.Select(r=>r.Roles);
            //End for User Types

            ViewBag.Region = _context.tbl_region.Where(r => r.id == currentUser.tbl_region_id).Select(r => r.name).FirstOrDefault();
            ViewBag.Province = _context.tbl_province.Where(p => p.id == currentUser.tbl_province_id).Select(p => p.name).FirstOrDefault();
            ViewBag.City = _context.tbl_city.Where(c => c.id == currentUser.tbl_city_id).Select(c => c.name).FirstOrDefault();
            ViewBag.Brgy = _context.tbl_brgy.Where(b => b.id == currentUser.tbl_brgy_id).Select(b => b.name).FirstOrDefault();
            
            //Profile Photo Source
            bool profilePhotoExist = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == uid && p.is_active == true).Any();
            if (profilePhotoExist == true)
            {
                var profilePhoto = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == uid && p.is_active == true).FirstOrDefault();
                ViewBag.ProfilePhotoSource = profilePhoto.webPath + "/" + profilePhoto.filename;//"/Files/UserPhotos/" + profilePhoto.filename;
            }
            else
            {
                ViewBag.ProfilePhotoSource = "/assets/images/default-avatar.png";
            }
            //END for Profile Photo Source
                        
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePassword([FromBody] ManageProfileViewModel model)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var usrDB = _context.tbl_user.Where(u => u.id == loggedUserID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                // Retrieve the user based on your authentication logic (e.g., by username)
                var user = _context.tbl_user.Where(u=>u.id == loggedUserID).FirstOrDefault();

                //Verify Old Password
                if (model.OldPassword == EncryptDecrypt.ConvertToDecrypt(user.password))
                {                   

                    // Update the user's password
                    user.password = EncryptDecrypt.ConvertToEncrypt(model.NewPassword);
                    user.date_modified = DateTime.Now;

                    // Save changes to the database
                    _context.SaveChanges();
                                        
                    try
                    {
                        //Email
                        var emailTemplate = _context.tbl_email_template.Where(e => e.id == 43).FirstOrDefault(); //id = 43 - Password Changed
                        var subject = emailTemplate.email_subject;
                        var BODY = emailTemplate.email_content.Replace("{FirstName}", usrDB.first_name);
                        BODY = BODY.Replace("{DateTimeNow}", DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt"));
                        var body = BODY.Replace(Environment.NewLine, "<br/>");

                        EmailSender.SendEmailAsync(((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value, subject, body);
                    }
                    catch
                    {
                        Console.WriteLine("An error occured on sending Email");
                    }

                    return Json(new { success = true });

                }
                else
                {
                    return Json(new { success = false, error = "Invalid old password." });
                }
            }
            // If model validation fails, return error details, including variables causing errors
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            // If model validation fails, return error details
            return Json(new { success = false, errors});
        }

        [HttpPost]
        public IActionResult UploadProfilePhoto(ProfilePhotoUploadModel model)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
            {
                
                FileInfo fileInfo = new FileInfo(model.ProfilePhoto.FileName);
                
                //Foler Name : USER_id
                string folderName = "USER_" + loggedUserID;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePhoto.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/UserPhotos/" + folderName);
                string webPath = "/Files/UserPhotos/" + folderName;
                //create folder if not exist
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePhoto.CopyTo(stream);
                }

                bool profilePhotoExist = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == loggedUserID).Any();
                //if profile picture doesn't exist
                if(profilePhotoExist == false)
                {
                    var profilePicsDB = new tbl_profile_pictures();
                    profilePicsDB.file_type = fileInfo.Extension;
                    profilePicsDB.tbl_user_id = loggedUserID;
                    profilePicsDB.filename = fileName;
                    profilePicsDB.path = path;
                    profilePicsDB.webPath = webPath;
                    profilePicsDB.file_size = Convert.ToInt32(model.ProfilePhoto.Length);
                    profilePicsDB.is_active = true;
                    profilePicsDB.created_by = loggedUserID;
                    profilePicsDB.modified_by = loggedUserID;
                    profilePicsDB.date_created = DateTime.Now;
                    profilePicsDB.date_modified = DateTime.Now;
                    _context.tbl_profile_pictures.Add(profilePicsDB);
                    _context.SaveChanges();
                }
                // if profile picture exist, remove the previous picture and edit the current row on database
                else
                {
                    var profilePicsDB = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == loggedUserID).FirstOrDefault();
                    var oldInfo = profilePicsDB;
                    var oldPathWithFilename = Path.Combine(oldInfo.path, oldInfo.filename);
                    //Delete Old Profile Pic on Folder
                    if (System.IO.File.Exists(oldPathWithFilename))
                    {
                        System.IO.File.Delete(oldPathWithFilename);
                    }
                    profilePicsDB.file_type = fileInfo.Extension;
                    profilePicsDB.tbl_user_id = loggedUserID;
                    profilePicsDB.filename = fileName;
                    profilePicsDB.path = path;
                    profilePicsDB.webPath = webPath;
                    profilePicsDB.file_size = Convert.ToInt32(model.ProfilePhoto.Length);
                    profilePicsDB.is_active = true;
                    profilePicsDB.created_by = loggedUserID;
                    profilePicsDB.modified_by = loggedUserID;
                    profilePicsDB.date_created = DateTime.Now;
                    profilePicsDB.date_modified = DateTime.Now;
                    _context.SaveChanges();
                }
                
                var webPathWithFilename = webPath + "/" + fileName;

                return Ok(new { webPathWithFilename });
            }

            return BadRequest("Invalid file.");
        }

        [HttpGet, ActionName("GetProfilePicSource")]
        public JsonResult GetProfilePicSource()
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string profilePhotoSource = "/assets/images/default-avatar.png";

            //Profile Photo Source
            bool profilePhotoExist = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == loggedUserID && p.is_active == true).Any();
            if (profilePhotoExist == true)
            {
                var profilePhoto = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == loggedUserID && p.is_active == true).FirstOrDefault();
                if (Directory.Exists(profilePhoto.path) == true)
                {
                    profilePhotoSource = profilePhoto.webPath + "/" + profilePhoto.filename;
                }
            }
            //END for Profile Photo Source

            return Json(profilePhotoSource);
        }
    }
}
