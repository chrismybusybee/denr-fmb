using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System;

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
                var userinfoList = from u in _context.tbl_user
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
                                       status = (bool)u.status
                                       //brgy = brngy.name
                                   };

                model.acctList = userinfoList;
                return View(model);
            }
        }

        public IActionResult EditAccount()
        {
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //CHECK IF USER STATUS IS APPROVED OR NOT
            bool? status = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault(); 
            if (uid == null || status == true)
            {
                return RedirectToAction("Index", "AccountManagement");
            }
            else
            {
                ViewModel model = new ViewModel();


                model.tbl_User = _context.tbl_user.Find(uid);
                //model.tbl_User.id = uid;
                //Generate List of Regions from the Database
                var regionList = _context.tbl_region.ToList();
                List<tbl_region> regions = new List<tbl_region>();

                foreach (var regList in regionList)
                {
                    regions.Add(new tbl_region { name = regList.name, id = regList.id, regCode = regList.regCode });
                }
                model.tbl_Regions = regions;
                //End for region

                //Generate List of Provinces from the Database
                var provinceList = _context.tbl_province.ToList();
                List<tbl_province> provinces = new List<tbl_province>();

                foreach (var provList in provinceList)
                {
                    provinces.Add(new tbl_province { name = provList.name, id = provList.id, regCode = provList.regCode, provCode = provList.provCode });
                }
                model.tbl_Provinces = provinces;
                //End for provinces

                //Generate List of City from the Database
                var cityList = _context.tbl_city.ToList();
                List<tbl_city> cities = new List<tbl_city>();

                foreach (var ctList in cityList)
                {
                    cities.Add(new tbl_city { name = ctList.name, id = ctList.id, regCode = ctList.regCode, provCode = ctList.provCode, citymunCode = ctList.citymunCode });
                }
                model.tbl_Cities = cities;
                //End for cities

                //Generate List of Barangay from the Database
                var brgyList = _context.tbl_brgy.ToList();
                List<tbl_brgy> brgys = new List<tbl_brgy>();

                foreach (var bgyList in brgyList)
                {
                    brgys.Add(new tbl_brgy { name = bgyList.name, id = bgyList.id, regCode = bgyList.regCode, provCode = bgyList.provCode, citymunCode = bgyList.citymunCode, brgyCode = bgyList.brgyCode });
                }
                model.tbl_Brgys = brgys;
                //End for barangays

                //File Paths from Database
                var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == uid).ToList();
                List<tbl_files> files = new List<tbl_files>();

                foreach (var fileList in filesFromDB)
                {
                    files.Add(new tbl_files { filename = fileList.filename, path = fileList.path });
                    //files.Add(new tbl_files { filename = f });
                }

                model.tbl_Files = files;
                //END FOR FILE DOWNLOAD

                return View(model);
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
            usrDB.first_name = model.tbl_User.first_name;
            usrDB.middle_name = model.tbl_User.middle_name;
            usrDB.last_name = model.tbl_User.last_name;
            usrDB.suffix = model.tbl_User.suffix;
            usrDB.contact_no = model.tbl_User.contact_no;
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
                                        comment = u.comment}).FirstOrDefault();

                    //mymodel.acctApprovalViewModels = (AcctApprovalViewModel?)userinfoList;

                    //File Paths from Database
                    var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == usid).ToList();
                    List<tbl_files> files = new List<tbl_files>();

                    foreach (var fileList in filesFromDB)
                    {
                        files.Add(new tbl_files { filename = fileList.filename, path = fileList.path });
                        //files.Add(new tbl_files { filename = f });
                    }

                    mymodel.tbl_Files = files;
                    //END FOR FILE DOWNLOAD

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
        public IActionResult AccountsApproval(int? uid, string SubmitButton, ViewModel model)
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
                string buttonClicked = SubmitButton;
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
                    var subject = "Permit to Import Application Status";
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
                    var subject = "Permit to Import Application Status";
                    var body = "Greetings! \n We regret to inform you that your Account has been rejected.\nThe officer left the following comment:\n" + model.acctApprovalViewModels.comment;
                    EmailSender.SendEmailAsync(model.acctApprovalViewModels.email, subject, body);
                }

                return RedirectToAction("Index", "AccountManagement");
            }

        }


    }
}