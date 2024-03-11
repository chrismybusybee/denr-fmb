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

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class AccountManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment EnvironmentWebHost;

        public AccountManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender, IWebHostEnvironment _environment)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
            EnvironmentWebHost = _environment;
        }

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

        [RequiresAccess(allowedAccessRights = "allow_page_manage_users")]
        public IActionResult Index()
        {
            //if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            //{
            //    return RedirectToAction("EditAccount", "AccountManagement");
            //}
            //else
            //{
                ViewModel model = new ViewModel();
                //var userinfoList                    
                model.acctList = (from u in _context.tbl_user

                                  join utypes in _context.tbl_user_type_user on u.id equals utypes.user_id

                                  join utype in _context.tbl_user_types on utypes.user_type_id equals utype.id
                                  //join reg in _context.tbl_region on u.tbl_region_id equals reg.id
                                  //join prov in _context.tbl_province on u.tbl_province_id equals prov.id
                                  //join ct in _context.tbl_city on u.tbl_city_id equals ct.id
                                  //join brngy in _context.tbl_brgy on u.tbl_brgy_id equals brngy.id
                                  select new AcctApprovalViewModel
                                  {
                                      FullName = u.first_name + " " + u.middle_name + " " + u.last_name + " " + u.suffix,
                                      userID = (int)u.id,
                                      //  userType = utype.name,
                                      email = u.email,
                                      contact_no = u.contact_no,
                                      //valid_id = u.valid_id,
                                      //valid_id_no = u.valid_id_no,
                                      //birth_date = u.birth_date,
                                      street_address = u.street_address,
                                      //region = reg.name,
                                      //province = prov.name,
                                      // city = ct.name,
                                      status = (bool)u.status,
                                      date_created = u.date_created,
                                      //brgy = brngy.name
                                      is_active = (bool)u.is_active
                                  }).OrderByDescending(u => u.date_created).GroupBy(u => u.userID).Select(group => group.First());
                //OrderByDescending(d => d.date_created)
                //model.acctList = userinfoList;
                return View(model);
            //}
        }
        public IActionResult AddAccount()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            if (usrRoleID == 13 || usrRoleID == 14) // Admin and Super Admin
            {
                ViewModel model = new ViewModel();
                var _regions = _context.tbl_region.ToList();
                var _provinces = new List<tbl_province>();
                var _cities = new List<tbl_city>();
                var _barangays = new List<tbl_brgy>();


                _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
                _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
                _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });
                _barangays.Add(new tbl_brgy() { id = 0, name = "-- Select Barangay --" });


                ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");
                if (ViewData["ProvinceData"] == null)
                {
                    ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.id), "id", "name");
                }
                if (ViewData["CityData"] == null)
                {
                    ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.id), "id", "name");
                }
                if (ViewData["BrgyData"] == null)
                {
                    ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.id), "id", "name");
                }

                model.tbl_Division_List = _context.tbl_division.Where(e => e.is_active == true).ToList();

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "AccountManagement");
            }

        }
        public IActionResult RequestToChangeAccountInfo()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var usInfo = _context.tbl_user.Where(u => u.id == uid).SingleOrDefault();
            //bool? status = usInfo.status;
            ViewModel model = new ViewModel();


            model.tbl_User = _context.tbl_user.Find(uid);
            //model.tbl_User.id = uid;

            var _regions = _context.tbl_region.ToList();
            var _provinces = _context.tbl_province.Where(p => p.regCode == usInfo.tbl_region_id).ToList();
            var _cities = _context.tbl_city.Where(c => c.provCode == usInfo.tbl_province_id).ToList();
            var _barangays = _context.tbl_brgy.Where(b => b.citymunCode == usInfo.tbl_city_id).ToList();
            //var _provinces = new List<tbl_province>();
            //var _cities = new List<tbl_city>();
            //var _barangays = new List<tbl_brgy>();

            _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
            _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
            _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });
            _barangays.Add(new tbl_brgy() { id = 0, name = "-- Select Barangay --" });

            ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");

            ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.name), "id", "name");
            ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.name), "id", "name");
            ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.name), "id", "name");

            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            ViewData["BaseUrl"] = host;

            //Get list of required documents from tbl_announcement
            var requirements = _context.tbl_announcement.Where(a => a.id == 1).FirstOrDefault();
            model.soloAnnouncement = requirements;
            //End for required documents

            //File Paths from Database
            var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == uid && !f.path.Contains("UserPhotos")).ToList();
            List<tbl_files> files = new List<tbl_files>();

            foreach (var fileList in filesFromDB)
            {
                files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, date_created = fileList.date_created, file_size = fileList.file_size });
                //files.Add(new tbl_files { filename = f });
            }

            model.tbl_Files = files;
            //END FOR FILE DOWNLOAD

            //Profile Photo Source
            bool profilePhotoExist = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).Any();
            if (profilePhotoExist == true)
            {
                var profilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).FirstOrDefault();
                ViewBag.profilePhotoSource = "/Files/UserPhotos/" + profilePhoto.filename;
            }
            else
            {
                ViewBag.profilePhotoSource = "/assets/images/default-avatar.png";
            }
            //END for Profile Photo Source

            //Display List of Comments
            model.commentsViewModelsList = (from c in _context.tbl_comments
                                            where c.tbl_user_id == uid
                                            join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                            join usr in _context.tbl_user on c.created_by equals usr.id
                                            select new CommentsViewModel
                                            {
                                                tbl_user_id = c.tbl_user_id,
                                                tbl_files_id = c.tbl_files_id,
                                                fileName = f.filename,
                                                comment = c.comment,
                                                commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                created_by = c.created_by,
                                                modified_by = c.modified_by,
                                                date_created = c.date_created,
                                                date_modified = c.date_modified
                                            }).OrderBy(f => f.fileName).ThenByDescending(d => d.date_created);
            return View(model);
        }
        public IActionResult RequestToChangeAccountInfoSuccess(int id)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var reqInfo = _context.tbl_user_change_info_request.Where(cui => cui.id == id).FirstOrDefault();
            if (id != null)
            {
                if (loggedUserID == reqInfo.tbl_user_id)
                {
                    ViewModel model = new ViewModel();
                    model.tbl_User_Change_Info_Request = reqInfo;
                    return View(model);
                }
                else
                {
                    return RedirectToAction("RequestToChangeAccountInfo", "AccountManagement");
                }
            }
            else
            {
                return RedirectToAction("RequestToChangeAccountInfo", "AccountManagement");
            }

        }
        public IActionResult EditAccount()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //CHECK IF USER STATUS IS APPROVED OR NOT
            var usInfo = _context.tbl_user.Where(u => u.id == uid).SingleOrDefault();
            bool? status = usInfo.status;
            //bool? status = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault(); 
            if (uid == null || status == true)
            {
                //return RedirectToAction("Index", "AccountManagement");
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewModel model = new ViewModel();


                model.tbl_User = _context.tbl_user.Find(uid);
                //model.tbl_User.id = uid;

                var _regions = _context.tbl_region.ToList();
                var _provinces = _context.tbl_province.Where(p => p.regCode == usInfo.tbl_region_id).ToList();
                var _cities = _context.tbl_city.Where(c => c.provCode == usInfo.tbl_province_id).ToList();
                var _barangays = _context.tbl_brgy.Where(b => b.citymunCode == usInfo.tbl_city_id).ToList();
                //var _provinces = new List<tbl_province>();
                //var _cities = new List<tbl_city>();
                //var _barangays = new List<tbl_brgy>();

                _regions.Add(new tbl_region() { id = 0, name = "--Select Region--" });
                _provinces.Add(new tbl_province() { id = 0, name = "--Select Province--" });
                _cities.Add(new tbl_city() { id = 0, name = "--Select City/Municipality--" });
                _barangays.Add(new tbl_brgy() { id = 0, name = "-- Select Barangay --" });

                ViewData["RegionData"] = new SelectList(_regions.OrderBy(s => s.id), "id", "name");

                ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.name), "id", "name");
                ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.name), "id", "name");
                ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.name), "id", "name");

                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                //Get list of required documents from tbl_announcement
                var requirements = _context.tbl_announcement.Where(a => a.id == 1).FirstOrDefault();
                model.soloAnnouncement = requirements;
                //End for required documents

                //File Paths from Database
                var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == uid && !f.path.Contains("UserPhotos")).ToList();
                List<tbl_files> files = new List<tbl_files>();

                foreach (var fileList in filesFromDB)
                {
                    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, date_created = fileList.date_created, file_size = fileList.file_size });
                    //files.Add(new tbl_files { filename = f });
                }

                model.tbl_Files = files;
                //END FOR FILE DOWNLOAD

                //Profile Photo Source
                bool profilePhotoExist = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).Any();
                if (profilePhotoExist == true)
                {
                    var folderName = "USER_" + uid;
                    var profilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).FirstOrDefault();
                    ViewBag.profilePhotoSource = "/Files/UserPhotos/" + folderName + "/" + profilePhoto.filename;
                }
                else
                {
                    ViewBag.profilePhotoSource = "/assets/images/default-avatar.png";
                }
                //END for Profile Photo Source

                //Display List of Comments
                model.commentsViewModelsList = (from c in _context.tbl_comments
                                                where c.tbl_user_id == uid
                                                join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                join usr in _context.tbl_user on c.created_by equals usr.id
                                                select new CommentsViewModel
                                                {
                                                    tbl_user_id = c.tbl_user_id,
                                                    tbl_files_id = c.tbl_files_id,
                                                    fileName = f.filename,
                                                    comment = c.comment,
                                                    commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                    created_by = c.created_by,
                                                    modified_by = c.modified_by,
                                                    date_created = c.date_created,
                                                    date_modified = c.date_modified
                                                }).OrderBy(f => f.fileName).ThenByDescending(d => d.date_created);
                return View(model);
            }

        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            model.isSuccess = false;
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

        [HttpPost]
        public IActionResult AddAccount(ViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";

            //Check if email exist
            bool emailExist = _context.tbl_user.Where(u => u.email == model.tbl_User.email).Any();
            if (emailExist == true)
            {
                ModelState.AddModelError("tbl_User.email", "Email Address already registered to the system, please try a different Email Address.");

                //LOAD BACK THE SELECTED PROVINCE, CITIES, AND BRGY
                var _provinces = _context.tbl_province.Where(p => p.regCode == model.tbl_User.tbl_region_id).ToList();
                var _cities = _context.tbl_city.Where(c => c.provCode == model.tbl_User.tbl_province_id).ToList();
                var _barangays = _context.tbl_brgy.Where(b => b.citymunCode == model.tbl_User.tbl_city_id).ToList();
                ViewData["ProvinceData"] = new SelectList(_provinces.OrderBy(s => s.name), "id", "name");
                ViewData["CityData"] = new SelectList(_cities.OrderBy(s => s.name), "id", "name");
                ViewData["BrgyData"] = new SelectList(_barangays.OrderBy(s => s.name), "id", "name");
                return View(model);
            }
            else
            {
                //ENCRYPT PASSWORD
                string decrPw = model.tbl_User.password;
                string encrPw = EncryptDecrypt.ConvertToEncrypt(model.tbl_User.password);
                //SET VALUES FOR VARIABLES WITHOUT INPUT FIELDS ON VIEW
                model.tbl_User.password = encrPw;
                model.tbl_User.status = true;
                //model.tbl_Users.comment;
                model.tbl_User.date_created = DateTime.Now;
                model.tbl_User.date_modified = DateTime.Now;
                //model.tbl_Users.tbl_user_types_id = Convert.ToInt32(model.tbl_Users.tbl_user_types_id);

                model.tbl_User.company_name = null;
                model.tbl_User.user_classification = "Individual";
                model.tbl_User.is_active = true;

                //Save Info to Database
                _context.tbl_user.Add(model.tbl_User);
                _context.SaveChanges();

                tbl_user_type_user _tbl_user_type_user = new tbl_user_type_user();
                
                        _tbl_user_type_user.user_type_id = model.tbl_User.tbl_user_types_id;
                        _tbl_user_type_user.user_id = model.tbl_User.id;
                        _tbl_user_type_user.is_active = true;
                        _tbl_user_type_user.created_by = uid;
                        _tbl_user_type_user.modified_by = uid;
                        _tbl_user_type_user.date_created = DateTime.Now;
                        _tbl_user_type_user.date_modified = DateTime.Now;

                _context.tbl_user_type_user.Add(_tbl_user_type_user);
                _context.SaveChanges();

                //var tblUsrTempPass = new tbl_user_temp_passwords();
                //tblUsrTempPass.tbl_user_id = model.tbl_User.id;
                //tblUsrTempPass.password = encrPw;
                //tblUsrTempPass.is_active = false;
                //tblUsrTempPass.date_created = DateTime.Now;
                //tblUsrTempPass.date_modified = DateTime.Now;
                //_context.tbl_user_temp_passwords.Add(tblUsrTempPass);
                //_context.SaveChanges();

                var subject = "Account Created";
                var body = "We would like to inform you that an admin created an account using your email for FMB-CIS.\nPlease login with your temporary password: " + decrPw + "\nPleace replace your password by visiting " + host + "ProfileManagement after you login. Thank You!";
                EmailSender.SendEmailAsync(model.tbl_User.email, subject, body);

                LogUserActivity("AccountManagement", "Add account", $"New Account Created: {model.tbl_User.email}", apkDateTime: DateTime.Now);
                //return View(model);
                return RedirectToAction("Index", "AccountManagement");
            }

        }

        [HttpPost]
        public IActionResult EditAccount(ViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //var usr = new tbl_user() {status = true, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };

            //using (_context)
            //{
            //    _context.tbl_user.Attach(usr);
            //    _context.Entry(usr).Property(x => x.status).IsModified = true;
            //    _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
            //    _context.Entry(usr).Property(x => x.comment).IsModified = true;
            //    _context.SaveChanges();
            //}

            var usrDB = _context.tbl_user.Where(m => m.id == uid).FirstOrDefault();
            usrDB.tbl_user_types_id = model.tbl_User.tbl_user_types_id;
            usrDB.user_classification = model.tbl_User.user_classification;
            usrDB.first_name = model.tbl_User.first_name;
            usrDB.middle_name = model.tbl_User.middle_name;
            usrDB.last_name = model.tbl_User.last_name;
            usrDB.suffix = model.tbl_User.suffix;
            if (model.tbl_User.user_classification == "Corporation" || model.tbl_User.user_classification == "Corporate/Cooperative")
            {
                usrDB.company_name = model.tbl_User.company_name;
            }
            usrDB.contact_no = model.tbl_User.contact_no;
            usrDB.birth_date = model.tbl_User.birth_date;
            usrDB.valid_id = model.tbl_User.valid_id;
            usrDB.valid_id_no = model.tbl_User.valid_id_no;
            usrDB.street_address = model.tbl_User.street_address;
            usrDB.tbl_region_id = model.tbl_User.tbl_region_id;
            usrDB.tbl_province_id = model.tbl_User.tbl_province_id;
            usrDB.tbl_city_id = model.tbl_User.tbl_city_id;
            usrDB.tbl_brgy_id = model.tbl_User.tbl_brgy_id;

            _context.Update(usrDB);
            _context.SaveChanges();
            //File Upload
            if (model.filesUpload != null)
            {

                var folderName = "USER_" + uid;
                foreach (var file in model.filesUpload.Files)
                {
                    var filesDB = new tbl_files();
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string path = Path.Combine(EnvironmentWebHost.ContentRootPath, "wwwroot/Files/UserDocs/" + folderName);

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string fileNameWithPath = Path.Combine(path, file.FileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    filesDB.tbl_user_id = uid;
                    filesDB.created_by = uid;
                    filesDB.modified_by = uid;
                    filesDB.date_created = DateTime.Now;
                    filesDB.date_modified = DateTime.Now;
                    filesDB.filename = file.FileName;
                    filesDB.path = path;
                    filesDB.tbl_file_type_id = fileInfo.Extension;
                    filesDB.tbl_file_sources_id = fileInfo.Extension;
                    filesDB.file_size = Convert.ToInt32(file.Length);
                    filesDB.version = 1;
                    _context.tbl_files.Add(filesDB);
                    _context.SaveChanges();
                }
            }
            //Check if profilePhotoExist is true
            bool profilePhotoExist = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos")).Any();
            if (profilePhotoExist == true)
            {
                var existingProfilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos")).FirstOrDefault();
                existingProfilePhoto.is_active = false;
                _context.Update(existingProfilePhoto);
                _context.SaveChanges();
            }
            //Profile Pic Upload
            if (model.profilePicUpload != null)
            {

                var folderName = "USER_" + uid;
                foreach (var pPicFile in model.profilePicUpload.Files)
                {
                    var profilePicFilesDB = new tbl_files();
                    FileInfo pPicFileInfo = new FileInfo(pPicFile.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/UserPhotos/" + folderName);

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string fileNameWithPath = Path.Combine(path, pPicFile.FileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        pPicFile.CopyTo(stream);
                    }
                    profilePicFilesDB.tbl_user_id = uid;
                    profilePicFilesDB.created_by = uid;
                    profilePicFilesDB.modified_by = uid;
                    profilePicFilesDB.date_created = DateTime.Now;
                    profilePicFilesDB.date_modified = DateTime.Now;
                    profilePicFilesDB.filename = pPicFile.FileName;
                    profilePicFilesDB.path = path;
                    profilePicFilesDB.tbl_file_type_id = pPicFileInfo.Extension;
                    profilePicFilesDB.tbl_file_sources_id = pPicFileInfo.Extension;
                    profilePicFilesDB.file_size = Convert.ToInt32(pPicFile.Length);
                    profilePicFilesDB.is_active = true;
                    _context.tbl_files.Add(profilePicFilesDB);
                    _context.SaveChanges();
                }
            }
            //END FOR Profile Pic UPLOAD

            //Email
            var subject = "User Registration Status";
            var body = "Greetings! \n We would like to inform you have successfully edited some information on your User Account.";
            EmailSender.SendEmailAsync(usrDB.email, subject, body);

            LogUserActivity("AccountManagement", "Edit account", $"Account Edited", apkDateTime: DateTime.Now);
            return RedirectToAction("Index", "AccountManagement");
        }

        [HttpPost]
        public IActionResult RequestToChangeAccountInfo(ViewModel model)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            var newChangeInfoRequest = new tbl_user_change_info_request();


            newChangeInfoRequest.tbl_user_id = loggedUserID;
            newChangeInfoRequest.ticket_no = "CIS-CUI" + DateTime.Now.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("HHmmssffffff");
            newChangeInfoRequest.first_name = model.tbl_User.first_name;
            newChangeInfoRequest.middle_name = model.tbl_User.middle_name;
            newChangeInfoRequest.last_name = model.tbl_User.last_name;
            newChangeInfoRequest.suffix = model.tbl_User.suffix;
            newChangeInfoRequest.company_name = model.tbl_User.company_name;
            newChangeInfoRequest.contact_no = model.tbl_User.contact_no;
            newChangeInfoRequest.valid_id = model.tbl_User.valid_id;
            newChangeInfoRequest.valid_id_no = model.tbl_User.valid_id_no;
            newChangeInfoRequest.birth_date = model.tbl_User.birth_date;
            newChangeInfoRequest.tbl_region_id = model.tbl_User.tbl_region_id;
            newChangeInfoRequest.tbl_province_id = model.tbl_User.tbl_province_id;
            newChangeInfoRequest.tbl_city_id = model.tbl_User.tbl_city_id;
            newChangeInfoRequest.tbl_brgy_id = model.tbl_User.tbl_brgy_id;
            newChangeInfoRequest.street_address = model.tbl_User.street_address;
            newChangeInfoRequest.email = model.tbl_User.email;
            newChangeInfoRequest.tbl_user_types_id = model.tbl_User.tbl_user_types_id;
            newChangeInfoRequest.user_classification = model.tbl_User.user_classification;
            newChangeInfoRequest.gender = model.tbl_User.gender;
            newChangeInfoRequest.tbl_user_change_info_request_status_id = 2; // 1 - Approved, 2 - Pending, 3 - Declined
            newChangeInfoRequest.created_by = loggedUserID;
            newChangeInfoRequest.modified_by = loggedUserID;
            newChangeInfoRequest.date_created = DateTime.Now;
            newChangeInfoRequest.date_modified = DateTime.Now;

            _context.tbl_user_change_info_request.Add(newChangeInfoRequest);
            _context.SaveChanges();

            //Email
            var subject = "Request to Change Account Information Status";
            var body = "Greetings! \n We would like to inform your request to change account information has been received.";
            EmailSender.SendEmailAsync(model.tbl_User.email, subject, body);

            LogUserActivity("AccountManagement", "Edit Account Request", $"Account Edit Request Created", apkDateTime: DateTime.Now);
            //return View(model);
            return RedirectToAction("RequestToChangeAccountInfoSuccess", "AccountManagement", new { id = newChangeInfoRequest.id });
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            string decrPw = model.NewPassword;
            string encrPw = EncryptDecrypt.ConvertToEncrypt(decrPw);

            var usrDB = _context.tbl_user.Where(m => m.id == uid).FirstOrDefault();

            if (model.OldPassword == EncryptDecrypt.ConvertToDecrypt(usrDB.password))
            {
                usrDB.password = encrPw;
                _context.Update(usrDB);
                _context.SaveChanges();
                model.isSuccess = true;
                LogUserActivity("AccountManagement", "Change Password", $"Password Changed", apkDateTime: DateTime.Now);
            }
            else
            {
                ModelState.AddModelError("", "Invalid Password");
                model.isSuccess = false;
            }
            return View(model);
        }

        //LIST OF ACCOUNTS CAN BE ACCESSED BY CENRO ON ACCOUNTS LIST
        public IActionResult AccountsList()
        {
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {

                ViewModel model = new ViewModel();
                //Get the list of users
                var userList = _context.tbl_user.ToList();
                model.tbl_Users = userList;

                return View(model);
            }

        }


        public IActionResult RequestToChangeAcctInfoList()
        {
            string roleOfLoggedUser = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
            int usrTypeID = _context.tbl_user_types.Where(utype => utype.name == roleOfLoggedUser).Select(utype => utype.id).FirstOrDefault();

            if (roleOfLoggedUser.Contains("Chainsaw") == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (usrTypeID == 8 || usrTypeID == 9 || usrTypeID == 13 || usrTypeID == 14 || usrTypeID == 17)
            {
                //Only the following user roles can access this page:
                //8 - DENR CENRO,
                //9 - DENR Implementing PENRO,
                //13 - DENR CIS Administrator,
                //14 - DENR CIS Super Admin,
                //17 - DENR Regional Executive Director(RED)

                ViewModel model = new ViewModel();

                //Set condition of which User roles can be viewed
                string condition = "";
                if (roleOfLoggedUser.Contains("Administrator") == true || roleOfLoggedUser.Contains("Super Admin") == true)
                {
                    condition = "DENR";
                }
                else if (roleOfLoggedUser.Contains("CENRO") == true || roleOfLoggedUser.Contains("Implementing PENRO") == true || roleOfLoggedUser.Contains("RED") == true)
                {
                    condition = "Chainsaw";
                    //CENRO, Implementing PENRO, and RED are the only allowed to view request for account changes of Chainsaw Owners
                }
                //model.RequestChangeAcctInfoViewModelList = (from rcui in _context.tbl_user_change_info_request
                //                                            join utype in _context.tbl_user_type_user on rcui.tbl_user_types_id equals utype.user_type_id
                //                                            join reqstat in _context.tbl_user_change_info_request_status on rcui.tbl_user_change_info_request_status_id equals reqstat.id
                //                                            join reg in _context.tbl_region on rcui.tbl_region_id equals reg.id
                //                                            join prov in _context.tbl_province on rcui.tbl_province_id equals prov.id
                //                                            join ct in _context.tbl_city on rcui.tbl_city_id equals ct.id
                //                                            join brngy in _context.tbl_brgy on rcui.tbl_brgy_id equals brngy.id
                //                                            select new RequestChangeAcctInfoViewModel
                //                                            {
                //                                                id = rcui.id,
                //                                                ticket_no = rcui.ticket_no,
                //                                                FullName = rcui.first_name + " " + rcui.middle_name + " " + rcui.last_name + " " + rcui.suffix,
                //                                                tbl_user_id = rcui.tbl_user_id,
                //                                                // userType = utype.,
                //                                                email = rcui.email,
                //                                                contact_no = rcui.contact_no,
                //                                                street_address = rcui.street_address,
                //                                                city = ct.name,
                //                                                status = reqstat.status_name,
                //                                                date_created = rcui.date_created
                //                                            }).Distinct().Where(rcui => rcui.userType.Contains(condition)).OrderByDescending(rcui => rcui.ticket_no).ToList();


                var query = (from rcui in _context.tbl_user_change_info_request
                             join u in _context.tbl_user on rcui.tbl_user_id equals u.id
                             join utypes in _context.tbl_user_type_user on u.id equals utypes.user_id
                             join utype in _context.tbl_user_types on utypes.user_type_id equals utype.id
                             join reqstat in _context.tbl_user_change_info_request_status on rcui.tbl_user_change_info_request_status_id equals reqstat.id
                             join reg in _context.tbl_region on rcui.tbl_region_id equals reg.id
                             join prov in _context.tbl_province on rcui.tbl_province_id equals prov.id
                             join ct in _context.tbl_city on rcui.tbl_city_id equals ct.id
                             join brngy in _context.tbl_brgy on rcui.tbl_brgy_id equals brngy.id
                             select new RequestChangeAcctInfoViewModel
                             {
                                 id = rcui.id,
                                 ticket_no = rcui.ticket_no,
                                 FullName = rcui.first_name + " " + rcui.middle_name + " " + rcui.last_name + " " + rcui.suffix,
                                 tbl_user_id = rcui.tbl_user_id,
                                 email = rcui.email,
                                 contact_no = rcui.contact_no,
                                 street_address = rcui.street_address,
                                 //  city = ct.name,
                                 status = reqstat.status_name,
                                 date_created = rcui.date_created,
                                 userType = utype.name // Replace with the actual property representing user types
                             }).OrderByDescending(rcui => rcui.id);

                // Applying filtering condition
                var filteredResult = query.AsEnumerable().GroupBy(rcui => rcui.ticket_no).Select(group => group.First()).ToList();


                model.RequestChangeAcctInfoViewModelList = filteredResult;
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        //TO BE USED BY CENRO/Implementing PENRO/RED/Admin/Super Admin FOR APPROVAL OF REQUEST IN CHANGING ACCOUNT INFO
        [HttpGet]
        public IActionResult RequestToChangeAcctInfoApproval(int id)
        {
            if (id == null)
            {
                return RedirectToAction("RequestToChangeAcctInfoList", "AccountManagement");

            }
            else
            {
                string roleOfLoggedUser = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
                int usrTypeID = _context.tbl_user_types.Where(utype => utype.name == roleOfLoggedUser).Select(utype => utype.id).FirstOrDefault();

                if (roleOfLoggedUser.Contains("Chainsaw") == true)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (usrTypeID == 8 || usrTypeID == 9 || usrTypeID == 13 || usrTypeID == 14 || usrTypeID == 17)
                {
                    //Only the following user roles can access this page:
                    //8 - DENR CENRO,
                    //9 - DENR Implementing PENRO,
                    //13 - DENR CIS Administrator,
                    //14 - DENR CIS Super Admin,
                    //17 - DENR Regional Executive Director(RED)

                    ViewModel model = new ViewModel();
                    if (id == null)
                    {
                        ModelState.AddModelError("", "Invalid Application");
                        return RedirectToAction("Index", "AccountManagement");
                    }

                    else
                    {
                        //int usid = Convert.ToInt32(uid);

                        model.RequestChangeAcctInfoViewModelApproval = (from rcui in _context.tbl_user_change_info_request
                                                                        where rcui.id == id
                                                                        join utbl in _context.tbl_user on rcui.tbl_user_id equals utbl.id
                                                                        join utypes in _context.tbl_user_type_user on utbl.id equals utypes.user_id

                                                                        join utype in _context.tbl_user_types on utypes.user_type_id equals utype.id
                                                                        join reqstat in _context.tbl_user_change_info_request_status on rcui.tbl_user_change_info_request_status_id equals reqstat.id
                                                                        join reg in _context.tbl_region on rcui.tbl_region_id equals reg.id
                                                                        join prov in _context.tbl_province on rcui.tbl_province_id equals prov.id
                                                                        join ct in _context.tbl_city on rcui.tbl_city_id equals ct.id
                                                                        join brngy in _context.tbl_brgy on rcui.tbl_brgy_id equals brngy.id
                                                                        select new RequestChangeAcctInfoViewModel
                                                                        {
                                                                            id = rcui.id,
                                                                            ticket_no = rcui.ticket_no,
                                                                            first_name = rcui.first_name,
                                                                            middle_name = rcui.middle_name,
                                                                            last_name = rcui.last_name,
                                                                            suffix = rcui.suffix,
                                                                            tbl_user_id = rcui.tbl_user_id,
                                                                            company_name = rcui.company_name,
                                                                            valid_id = rcui.valid_id,
                                                                            valid_id_no = rcui.valid_id_no,
                                                                            birth_date = rcui.birth_date,
                                                                            // userType = utype.name,
                                                                            tbl_user_types_id = rcui.tbl_user_types_id,
                                                                            user_classification = rcui.user_classification,
                                                                            gender = rcui.gender,
                                                                            email = rcui.email,
                                                                            contact_no = rcui.contact_no,
                                                                            street_address = rcui.street_address,
                                                                            brgy = brngy.name,
                                                                            city = ct.name,
                                                                            province = prov.name,
                                                                            region = reg.name,
                                                                            tbl_brgy_id = rcui.tbl_brgy_id,
                                                                            tbl_city_id = rcui.tbl_city_id,
                                                                            tbl_province_id = rcui.tbl_province_id,
                                                                            tbl_region_id = rcui.tbl_region_id,
                                                                            //status = reqstat.status_name,
                                                                            date_created = rcui.date_created
                                                                        }).FirstOrDefault();


                        model.acctApprovalViewModels = (from u in _context.tbl_user
                                                        where u.id == model.RequestChangeAcctInfoViewModelApproval.tbl_user_id
                                                        join utypes in _context.tbl_user_type_user on u.id equals utypes.user_id

                                                        join utype in _context.tbl_user_types on utypes.user_type_id equals utype.id
                                                        join reg in _context.tbl_region on u.tbl_region_id equals reg.id
                                                        join prov in _context.tbl_province on u.tbl_province_id equals prov.id
                                                        join ct in _context.tbl_city on u.tbl_city_id equals ct.id
                                                        join brngy in _context.tbl_brgy on u.tbl_brgy_id equals brngy.id
                                                        select new AcctApprovalViewModel
                                                        {
                                                            first_name = u.first_name,
                                                            middle_name = u.middle_name,
                                                            last_name = u.last_name,
                                                            suffix = u.suffix,
                                                            userType = utype.name,
                                                            email = u.email,
                                                            contact_no = u.contact_no,
                                                            valid_id = u.valid_id,
                                                            valid_id_no = u.valid_id_no,
                                                            birth_date = u.birth_date.ToString(),
                                                            street_address = u.street_address,
                                                            region = reg.name,
                                                            province = prov.name,
                                                            city = ct.name,
                                                            brgy = brngy.name,
                                                            comment = u.comment,
                                                            user_classification = u.user_classification,
                                                            gender = u.gender,
                                                            company_name = u.company_name
                                                        }).FirstOrDefault();

                        return View(model);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }


        }

        //TO BE USED BY CENRO FOR APPROVAL OF NEWLY REGISTERED ACCOUNTS (REMOVED)
        //TO BE USED BY SUPER ADMIN TO ENABLE/DISABLE ACCOUNTS (current purpose)
        [RequiresAccess(allowedAccessRights = "allow_page_manage_users")]
        [HttpGet]
        //[Url("?email={email}&code={code}")]
        public IActionResult AccountsApproval(string uid)
        {
            ViewModel mymodel = new ViewModel();
            int usid = Convert.ToInt32(uid);
            var userExist = _context.tbl_user.Any(u => u.id == usid);
            if (userExist == false) //Redirect to Dashboard if User doesn't exist
            {
                ModelState.AddModelError("", "Invalid User");
                return RedirectToAction("Index", "AccountManagement");
            }

            else
            {
                mymodel.acctApprovalViewModels = (from u in _context.tbl_user
                                                  where u.id == usid
                                                  join utypes in _context.tbl_user_type_user on u.id equals utypes.user_id

                                                  join utype in _context.tbl_user_types on utypes.user_type_id equals utype.id
                                                  join reg in _context.tbl_region on u.tbl_region_id equals reg.id
                                                  join prov in _context.tbl_province on u.tbl_province_id equals prov.id
                                                  join ct in _context.tbl_city on u.tbl_city_id equals ct.id
                                                  join brngy in _context.tbl_brgy on u.tbl_brgy_id equals brngy.id
                                                  select new AcctApprovalViewModel
                                                  {
                                                      FullName = u.first_name + " " + u.middle_name + " " + u.last_name + " " + u.suffix,
                                                      first_name = u.first_name,
                                                      middle_name = u.middle_name,
                                                      last_name = u.last_name,
                                                      suffix = u.suffix,
                                                      userType = utype.name,
                                                      email = u.email,
                                                      contact_no = u.contact_no,
                                                      valid_id = u.valid_id,
                                                      valid_id_no = u.valid_id_no,
                                                      birth_date = u.birth_date.ToString(),
                                                      street_address = u.street_address,
                                                      region = reg.name,
                                                      province = prov.name,
                                                      city = ct.name,
                                                      brgy = brngy.name,
                                                      comment = u.comment,
                                                      user_classification = u.user_classification,
                                                      gender = u.gender,
                                                      company_name = u.company_name,
                                                      is_active = u.is_active
                                                  }).FirstOrDefault();

                //User Types
                var myUserTypes = (from usrRoles in _context.tbl_user_type_user
                                   where usrRoles.user_id == usid && usrRoles.is_active == true
                                   join utypesName in _context.tbl_user_types on usrRoles.user_type_id equals utypesName.id
                                   select new
                                   {
                                       Roles = utypesName.name
                                   }).ToList();
                ViewBag.UserTypes = myUserTypes.Select(r => r.Roles);
                //End for User Types

                //File Paths from Database
                var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == usid && !f.path.Contains("UserPhotos")).ToList();
                List<tbl_files> files = new List<tbl_files>();

                foreach (var fileList in filesFromDB)
                {
                    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, date_created = fileList.date_created, file_size = fileList.file_size });
                    //files.Add(new tbl_files { filename = f });
                }

                mymodel.tbl_Files = files;
                //END FOR FILE DOWNLOAD

                //Profile Photo Source                    
                bool profilePhotoExist = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == usid && p.is_active == true).Any();
                if (profilePhotoExist == true)
                {
                    var profilePhoto = _context.tbl_profile_pictures.Where(p => p.tbl_user_id == usid && p.is_active == true).FirstOrDefault();
                    if (Directory.Exists(profilePhoto.path) == true)
                    {
                        ViewBag.profilePhotoSource = profilePhoto.webPath + "/" + profilePhoto.filename;
                    }
                    else
                    {
                        ViewBag.profilePhotoSource = "/assets/images/default-avatar.png";
                    }
                }
                else
                {
                    ViewBag.profilePhotoSource = "/assets/images/default-avatar.png";
                }
                //END for Profile Photo Source
                //Display List of Comments
                mymodel.commentsViewModelsList = (from c in _context.tbl_comments
                                                  where c.tbl_user_id == usid
                                                  //join f in _context.tbl_files on c.tbl_files_id equals f.Id into fileGroup
                                                  //from file in fileGroup.DefaultIfEmpty()
                                                  join usr in _context.tbl_user on c.created_by equals usr.id
                                                  select new CommentsViewModel
                                                  {
                                                      tbl_user_id = c.tbl_user_id,
                                                      tbl_files_id = c.tbl_files_id,
                                                      //fileName = file.filename,
                                                      comment = c.comment,
                                                      commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                      created_by = c.created_by,
                                                      modified_by = c.modified_by,
                                                      date_created = c.date_created,
                                                      date_modified = c.date_modified
                                                  }).OrderBy/*(f => f.fileName).ThenByDescending*/(d => d.date_created);

                return View(mymodel);
            }


        }

        //For File Download
        public FileResult DownloadFile(string fileName, string path)
        {
            //Build the File Path.
            string pathWithFilename = path + "//" + fileName;
            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(pathWithFilename);
            //Log Download Initiated
            LogUserActivity("Download", "Download File", $"File download initiated. {fileName}", apkDateTime: DateTime.Now);
            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public IActionResult RequestToChangeAcctInfoApproval(ViewModel model)
        {
            if (model.decision == "Approve") //Approve
            {
                var usrTbl = _context.tbl_user.Where(m => m.id == model.tbl_User_Change_Info_Request.tbl_user_id).FirstOrDefault();
                usrTbl.first_name = model.tbl_User_Change_Info_Request.first_name;
                usrTbl.middle_name = model.tbl_User_Change_Info_Request.middle_name;
                usrTbl.last_name = model.tbl_User_Change_Info_Request.last_name;
                usrTbl.suffix = model.tbl_User_Change_Info_Request.suffix;
                usrTbl.company_name = model.tbl_User_Change_Info_Request.company_name;
                usrTbl.contact_no = model.tbl_User_Change_Info_Request.contact_no;
                usrTbl.valid_id = model.tbl_User_Change_Info_Request.valid_id;
                usrTbl.valid_id_no = model.tbl_User_Change_Info_Request.valid_id_no;
                usrTbl.birth_date = model.tbl_User_Change_Info_Request.birth_date;
                usrTbl.tbl_region_id = Convert.ToInt32(model.tbl_User_Change_Info_Request.tbl_region_id);
                usrTbl.tbl_province_id = Convert.ToInt32(model.tbl_User_Change_Info_Request.tbl_province_id);
                usrTbl.tbl_city_id = Convert.ToInt32(model.tbl_User_Change_Info_Request.tbl_city_id);
                usrTbl.tbl_brgy_id = Convert.ToInt32(model.tbl_User_Change_Info_Request.tbl_brgy_id);
                usrTbl.street_address = model.tbl_User_Change_Info_Request.street_address;
                usrTbl.email = model.tbl_User_Change_Info_Request.email;
                usrTbl.tbl_user_types_id = Convert.ToInt32(model.tbl_User_Change_Info_Request.tbl_user_types_id);
                usrTbl.user_classification = model.tbl_User_Change_Info_Request.user_classification;
                usrTbl.date_modified = DateTime.Now;
                _context.Update(usrTbl);
                _context.SaveChanges();

                var reqTbl = _context.tbl_user_change_info_request.Where(m => m.id == model.tbl_User_Change_Info_Request.id).FirstOrDefault();
                reqTbl.tbl_user_change_info_request_status_id = 1;
                _context.Update(reqTbl);
                _context.SaveChanges();
                //Email
                var subject = "Request to Change Account Information Status";
                var body = "Greetings! \n We would like to inform your request to change account information has been approved.";
                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
                LogUserActivity("AccountManagement", "Edit Account Request Approved", $"Account Edit Request for {model.acctApprovalViewModels.email} has been approved", apkDateTime: DateTime.Now);
                return RedirectToAction("RequestToChangeAcctInfoList", "AccountManagement");

            }
            else //Decline
            {
                var reqTbl = _context.tbl_user_change_info_request.Where(m => m.id == model.tbl_User_Change_Info_Request.id).FirstOrDefault();
                reqTbl.tbl_user_change_info_request_status_id = 3;
                _context.Update(reqTbl);
                _context.SaveChanges();
                //Email
                var subject = "Request to Change Account Information Status";
                var body = "Greetings! \n We would like to inform your request to change account information has been declined.";
                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
                LogUserActivity("AccountManagement", "Edit Account Request Declined", $"Account Edit Request for {model.acctApprovalViewModels.email} has been declined", apkDateTime: DateTime.Now);
                return RedirectToAction("RequestToChangeAcctInfoApproval", "AccountManagement", new { id = model.tbl_User_Change_Info_Request.id });
                //return RedirectToAction("RequestToChangeAcctInfoList","AccountManagement");
            }
            //return View();
        }

        //[HttpPost]
        ////[Url("?email={email}&code={code}")]
        //public IActionResult AccountsApproval(int? uid, ViewModel model)
        //{
        //    int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

        //    string roleOfLoggedUser = ((ClaimsIdentity)User.Identity).FindFirst("userRole").Value;
        //    int usrTypeID = _context.tbl_user_types.Where(utype => utype.name == roleOfLoggedUser).Select(utype => utype.id).FirstOrDefault();

        //    //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
        //    //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();

        //    if (uid == null)
        //    {
        //        return View();
        //    }
        //    else
        //    {
        //        int usid = Convert.ToInt32(uid);
        //        string buttonClicked = model.decision;

        //        if (usrTypeID <= 7)
        //        {
        //            return RedirectToAction("Index", "AccountManagement");
        //        }
        //        else if (usrTypeID == 8 || usrTypeID == 9 || usrTypeID == 17)
        //        {
        //            //ONLY THE FOLLOWING ROLES CAN APPROVE OR DECLINE AN ACCOUNT REGISTRATION
        //            //8 - DENR CENRO,
        //            //9 - DENR Implementing PENRO,
        //            //17 - DENR Regional Executive Director(RED)
        //            if (buttonClicked == "Approve")
        //            {
        //                //Get email and subject from templates in DB
        //                var emailTemplate = _context.tbl_email_template.Where(e => e.id == 2).FirstOrDefault();
        //                // id = 2 - Account Registration (Notice of Acceptance)

        //                var usr = new tbl_user() { id = usid, status = true, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };

        //                using (_context)
        //                {
        //                    _context.tbl_user.Attach(usr);
        //                    _context.Entry(usr).Property(x => x.status).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.comment).IsModified = true;
        //                    _context.SaveChanges();
        //                }
        //                //Email
        //                //var subject = "Account Registration Status";
        //                //var body = "Greetings! \n We would like to inform your account has been approved.\nThe officer left the following comment:\n" + model.acctApprovalViewModels.comment;


        //                var subject = emailTemplate.email_subject;
        //                var BODY = emailTemplate.email_content.Replace("{FirstName}", model.acctApprovalViewModels.first_name);
        //                var body = BODY.Replace(Environment.NewLine, "<br/>");
        //                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
        //            }
        //            else //if (buttonClicked == "Decline")
        //            {
        //                //Get email and subject from templates in DB
        //                var emailTemplate = _context.tbl_email_template.Where(e => e.id == 3).FirstOrDefault();
        //                // id = 3 - Account Registration (Notice of Rejection)
        //                var usr = new tbl_user() { id = usid, status = false, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };
        //                using (_context)
        //                {
        //                    _context.tbl_user.Attach(usr);
        //                    _context.Entry(usr).Property(x => x.status).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.comment).IsModified = true;
        //                    _context.SaveChanges();
        //                }
        //                //Email
        //                //var subject = "Account Registration Status";
        //                //var body = "Greetings! \n We regret to inform you that your Account has been rejected.\nThe officer left the following comment:\n" + model.acctApprovalViewModels.comment;
        //                var subject = emailTemplate.email_subject;
        //                var BODY = emailTemplate.email_content.Replace("{FirstName}", model.acctApprovalViewModels.first_name);
        //                var body = BODY.Replace(Environment.NewLine, "<br/>");
        //                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
        //            }
        //        }
        //        else if (usrTypeID == 13 || usrTypeID == 14)
        //        {
        //            //ONLY THE FOLLOWING ROLES CAN APPROVE OR DECLINE AN ACCOUNT REGISTRATION
        //            //13 - DENR CIS Administrator,
        //            //14 - DENR CIS Super Admin,
        //            if (buttonClicked == "Enable")
        //            {
        //                var usr = new tbl_user() { id = usid, is_active = true, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };

        //                using (_context)
        //                {
        //                    _context.tbl_user.Attach(usr);
        //                    _context.Entry(usr).Property(x => x.is_active).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.comment).IsModified = true;
        //                    _context.SaveChanges();
        //                }
        //                //Email
        //                var subject = "CIS Account Status";
        //                var body = "Greetings! \n We would like to inform your account has been enabled.\nYou are now allowed to login in the CIS system. Thank you!";
        //                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
        //            }
        //            else //if (buttonClicked == "Disable")
        //            {
        //                var usr = new tbl_user() { id = usid, is_active = false, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };
        //                using (_context)
        //                {
        //                    _context.tbl_user.Attach(usr);
        //                    _context.Entry(usr).Property(x => x.is_active).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
        //                    _context.Entry(usr).Property(x => x.comment).IsModified = true;
        //                    _context.SaveChanges();
        //                }
        //                //Email
        //                var subject = "CIS Account Status";
        //                var body = "Greetings! \n We regret to inform you that your Account has been disabled.\nYou will not be allowed to login in the CIS system.";
        //                EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
        //            }
        //        }

        //        return RedirectToAction("Index", "AccountManagement");
        //    }

        //}

        [HttpPost]
        public IActionResult AccountsApproval(int? uid, ViewModel model)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            int usid = Convert.ToInt32(uid); // (user ID from URL parameter) Convert nullable int to int
            string buttonClicked = model.decision; // Enable/Disable of accounts
            var userInfo = _context.tbl_user.FirstOrDefault(u => u.id == usid); // Get the user info from database
            int emailTemplateID = 0; //To be initialized on if else statements

            if (buttonClicked == "Enable")
            {
                // Enable the user account by setting is_active to true
                userInfo.is_active = true;
                userInfo.date_modified = DateTime.Now;
                _context.SaveChanges();
                emailTemplateID = 45; //id = 45 - Account Enabled
                LogUserActivity("AccountManagement", "Account Enabled", $"Account Enabled for {userInfo.email}", apkDateTime: DateTime.Now);
            }
            else //if (buttonClicked == "Disable")
            {
                // Disable the user account by setting is_active to false
                userInfo.is_active = false;
                userInfo.date_modified = DateTime.Now;
                _context.SaveChanges();
                emailTemplateID = 44; //id = 44 - Account Disabled
                LogUserActivity("AccountManagement", "Account Disabled", $"Account Disabled for {userInfo.email}", apkDateTime: DateTime.Now);
            }

            try
            {
                //Email
                var emailTemplate = _context.tbl_email_template.Where(e => e.id == emailTemplateID).FirstOrDefault();
                var subject = emailTemplate.email_subject;
                var BODY = emailTemplate.email_content.Replace("{FirstName}", userInfo.first_name);
                var body = BODY.Replace(Environment.NewLine, "<br/>");

                //EmailSender.SendEmailAsync(((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value, subject, body);
                EmailSender.SendEmailAsync(userInfo.email, subject, body);
            }
            catch
            {
                Console.WriteLine("An error occured on sending Email");
            }
            return RedirectToAction("Index", "AccountManagement");
            

        }

        [HttpPost]
        public IActionResult CommentSection(int? uid, ViewModel model)
        {
            var commentsTbl = new tbl_comments();
            //commentsTbl.id = model.tbl_Comments.id;
            commentsTbl.tbl_user_id = Convert.ToInt32(uid);
            commentsTbl.tbl_files_id = model.tbl_Comments.tbl_files_id;
            commentsTbl.comment = model.tbl_Comments.comment;
            commentsTbl.created_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.modified_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.date_created = DateTime.Now;
            commentsTbl.date_modified = DateTime.Now;
            _context.tbl_comments.Add(commentsTbl);
            _context.SaveChanges();
            //return RedirectToAction("AccountsApproval?uid="+uid, "AccountManagement");
            //Url.Action("A","B",new{a="x"})
            LogUserActivity("AccountManagement", "New Comment", $"Added new comment", apkDateTime: DateTime.Now);
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            else
            {
                return RedirectToAction("AccountsApproval", "AccountManagement", new { uid = uid });
            }
        }

    }
}