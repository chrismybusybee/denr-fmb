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

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class AccountManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }

        public AccountManagementController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }
        public IActionResult Index()
        {
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            else
            {
                ViewModel model = new ViewModel();
                //var userinfoList                    
                model.acctList = (from u in _context.tbl_user
                                   join utype in _context.tbl_user_types on u.tbl_user_types_id equals utype.id
                                   join reg in _context.tbl_region on u.tbl_region_id equals reg.id
                                   join prov in _context.tbl_province on u.tbl_province_id equals prov.id
                                   join ct in _context.tbl_city on u.tbl_city_id equals ct.id
                                   join brngy in _context.tbl_brgy on u.tbl_brgy_id equals brngy.id
                                   select new AcctApprovalViewModel
                                   {
                                       FullName = u.first_name + " " + u.middle_name + " " + u.last_name + " " + u.suffix,
                                       userID = (int)u.id,
                                       userType = utype.name,
                                       email = u.email,
                                       contact_no = u.contact_no,
                                       //valid_id = u.valid_id,
                                       //valid_id_no = u.valid_id_no,
                                       //birth_date = u.birth_date,
                                       street_address = u.street_address,
                                       //region = reg.name,
                                       //province = prov.name,
                                       city = ct.name,
                                       status = (bool)u.status,
                                       date_created = u.date_created
                                       //brgy = brngy.name
                                   }).OrderByDescending(u => u.date_created);
                //OrderByDescending(d => d.date_created)
                //model.acctList = userinfoList;
                return View(model);
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
                bool profilePhotoExist = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos")).Any();
                if(profilePhotoExist == true)
                {
                    var profilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos")).FirstOrDefault();
                    ViewBag.profilePhotoSource = profilePhoto.path.Replace("/","\\") + "\\" + profilePhoto.filename;
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

        [HttpPost, ActionName("GetProvinceByRegionId")]
        public JsonResult GetProvinceByRegionId(string tbl_region_id)
        {
            int regID;
            List<tbl_province> provinceLists = new List<tbl_province>();
            if (!string.IsNullOrEmpty(tbl_region_id))
            {
                regID = Convert.ToInt32(tbl_region_id);
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
            usrDB.first_name = model.tbl_User.first_name;
            usrDB.middle_name = model.tbl_User.middle_name;
            usrDB.last_name = model.tbl_User.last_name;
            usrDB.suffix = model.tbl_User.suffix;
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
                    _context.tbl_files.Add(filesDB);
                    _context.SaveChanges();
                }
            }
            //Email
            var subject = "User Registration Status";
            var body = "Greetings! \n We would like to inform you have successfully edited some information on your User Account.";
            EmailSender.SendEmailAsync(usrDB.email, subject, body);
            return RedirectToAction("Index","AccountManagement");
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
        //TO BE USED BY CENRO FOR APPROVAL OF NEWLY REGISTERED ACCOUNTS
        [HttpGet]
        //[Url("?email={email}&code={code}")]
        public IActionResult AccountsApproval(string uid)
        {
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                //Temporary only
                return RedirectToAction("Index", "Dashboard");
                //Must allow external user to edit their own account.
            }
            else
            {

                ViewModel mymodel = new ViewModel();
                //tbl_user user = _context.tbl_user.Find(uid);
                if (uid == null)
                {
                    ModelState.AddModelError("", "Invalid Application");
                    return RedirectToAction("Index", "AccountManagement");
                }

                else
                {
                    int usid = Convert.ToInt32(uid);
                    //var UserList = _context.tbl_user.ToList();
                    //var UserInfo = UserList.Where(m => m.id == usid).ToList();

                    //ViewModel model = new ViewModel();

                    //var userinfoList
                    mymodel.acctApprovalViewModels = (from u in _context.tbl_user
                                   where u.id == usid
                                   join utype in _context.tbl_user_types on u.tbl_user_types_id equals utype.id
                                   join reg in _context.tbl_region on u.tbl_region_id equals reg.id
                                   join prov in _context.tbl_province on u.tbl_province_id equals prov.id
                                   join ct in _context.tbl_city on u.tbl_city_id equals ct.id
                                   join brngy in _context.tbl_brgy on u.tbl_brgy_id equals brngy.id
                                   select new AcctApprovalViewModel {FullName = u.first_name + " " + u.middle_name + " " + u.last_name + " " + u.suffix,
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
                                       gender = u.gender
                                   }).FirstOrDefault();

                    //mymodel.acctApprovalViewModels = (AcctApprovalViewModel?)userinfoList;

                    //File Paths from Database
                    var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == usid).ToList();
                    List<tbl_files> files = new List<tbl_files>();

                    foreach (var fileList in filesFromDB)
                    {
                        files.Add(new tbl_files { Id= fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, date_created = fileList.date_created, file_size = fileList.file_size });
                        //files.Add(new tbl_files { filename = f });
                    }

                    mymodel.tbl_Files = files;
                    //END FOR FILE DOWNLOAD

                    //Profile Photo Source
                    //var profilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == usid && f.path.Contains("UserPhotos")).FirstOrDefault();
                    //ViewBag.profilePhotoSource = profilePhoto.path + "\\" + profilePhoto.filename;
                    //END for Profile Photo Source

                    //TEST
                    /*
                    var commentsFromDB = _context.tbl_comments.Where(u => u.tbl_user_id == usid).ToList();
                    List<tbl_comments> comments = new List<tbl_comments>();
                    foreach (var commentsList in commentsFromDB)
                    {
                        comments.Add(new tbl_comments {id = commentsList.id  });
                    }*/
                    //Display List of Comments
                    mymodel.commentsViewModelsList = (from c in _context.tbl_comments
                                                      where c.tbl_user_id == usid
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
           
                    return View(mymodel);
                }
            }

        }

        //For File Download
        public FileResult DownloadFile(string fileName, string path)
        {
            //Build the File Path.
            string pathWithFilename = path + "//" + fileName;
            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(pathWithFilename);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        //[Url("?email={email}&code={code}")]
        public IActionResult AccountsApproval(int? uid, ViewModel model)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();

            if (uid == null)
            {
                return View();
            }
            else
            {
                int usid = Convert.ToInt32(uid);
                string buttonClicked = model.decision;
                if (buttonClicked == "Approve")
                {                    
                    var usr = new tbl_user() { id = usid, status = true, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };
                    
                    using (_context)
                    {
                        _context.tbl_user.Attach(usr);
                        _context.Entry(usr).Property(x => x.status).IsModified = true;
                        _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usr).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    var subject = "Account Registration Status";
                    var body = "Greetings! \n We would like to inform your account has been approved.\nThe officer left the following comment:\n" + model.acctApprovalViewModels.comment;
                    EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
                }
                else //if (buttonClicked == "Decline")
                {
                    var usr = new tbl_user() { id = usid, status = false, date_modified = DateTime.Now, comment = model.acctApprovalViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_user.Attach(usr);
                        _context.Entry(usr).Property(x => x.status).IsModified = true;
                        _context.Entry(usr).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usr).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    var subject = "Account Registration Status";
                    var body = "Greetings! \n We regret to inform you that your Account has been rejected.\nThe officer left the following comment:\n" + model.acctApprovalViewModels.comment;
                    EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
                }

                return RedirectToAction("Index", "AccountManagement");
            }

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