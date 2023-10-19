
using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Identity.UI.Services;




namespace FMB_CIS.Controllers
{
    [Authorize]
    public class ChainsawOwnerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LocalContext _context;
        private IEmailSender EmailSender { get; set; }


        public ChainsawOwnerController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }
        public IActionResult Index()
        {
            //Set Roles who can access this page
            int uid = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usrRoleID = _context.tbl_user.Where(u => u.id == uid).Select(u => u.tbl_user_types_id).SingleOrDefault();
            bool? usrStatus = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault();

            if (usrStatus != true) //IF User is not yet approved by the admin.
            {
                return RedirectToAction("Index", "Dashboard");
            }
            if (usrRoleID == 3 || usrRoleID == 5 || usrRoleID == 6 || usrRoleID == 7) 
            {
                return View();
            }
            else if (usrRoleID == 8 || usrRoleID == 9 || usrRoleID == 10 || usrRoleID == 11) //(((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("DENR") == true)
            {

                return RedirectToAction("ChainsawOwnerApplicantsList", "ChainsawOwner");
                
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ViewModel model)
        {
            //try
            //{
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                //DAL dal = new DAL();

                //SAVE permit application
                model.tbl_Application.tbl_application_type_id = 1;
                model.tbl_Application.status = 1;
                model.tbl_Application.tbl_user_id = userID;
                model.tbl_Application.is_active = true;
                model.tbl_Application.created_by = userID;
                model.tbl_Application.modified_by = userID;
                model.tbl_Application.date_created = DateTime.Now;
                model.tbl_Application.date_modified = DateTime.Now;

                _context.tbl_application.Add(model.tbl_Application);
                _context.SaveChanges();
                int? appID = model.tbl_Application.id;

                //File Upload
                if (model.filesUpload != null)
                {
                    foreach (var file in model.filesUpload.Files)
                    {
                        var filesDB = new tbl_files();
                        FileInfo fileInfo = new FileInfo(file.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/UserDocs");

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);


                        string fileNameWithPath = Path.Combine(path, file.FileName);

                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        filesDB.tbl_application_id = appID;
                        filesDB.created_by = userID;
                        filesDB.modified_by = userID;
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
                var subject = "Permit Application Status";
                var body = "Greetings! \n We would like to inform you that your Permit Application has been received.";
                EmailSender.SendEmailAsync(((ClaimsIdentity)User.Identity).FindFirst("EmailAdd").Value, subject, body);

                ModelState.Clear();
                ViewBag.Message = "Save Success";
                return View();
            }
            return View(model);
            //}
            //catch
            //{
            //    return View(model);
            //    //return RedirectToAction("Index", "Dashboard");
            //}
        }
        [HttpGet]
        //[Url("?email={email}&code={code}")]
        public IActionResult ChainsawOwnerApproval(string uid, string appid)
        {
            ViewModel mymodel = new ViewModel();
            //tbl_user user = _context.tbl_user.Find(uid);

            //CODE FOR FILE DOWNLOAD
            int applicID = Convert.ToInt32(appid);
            //File Paths from Database
            var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID).ToList();
            List<tbl_files> files = new List<tbl_files>();

            foreach (var fileList in filesFromDB)
            {
                files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                //files.Add(new tbl_files { filename = f });
            }

            mymodel.tbl_Files = files;
            //END FOR FILE DOWNLOAD

            if (uid == null || appid == null)
            {
                ModelState.AddModelError("", "Invalid Owner Application");
                return RedirectToAction("ChainsawOwnerApplicantsList", "ChainsawOwner");
            }

            else
            {

                int usid = Convert.ToInt32(uid);
                int applid = Convert.ToInt32(appid);
                //var UserList = _context.tbl_user.ToList();
                //var UserInfo = UserList.Where(m => m.id == usid).ToList();

                //ViewModel mymodel = new ViewModel();
                var applicationlist = from a in _context.tbl_application
                                      where a.tbl_user_id == usid && a.id == applid
                                      select a;

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;

                var applicationMod = (from a in applicationlist
                                      join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                      join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                      join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                      join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                      join pS in _context.tbl_permit_status on a.status equals pS.id
                                      join reg in _context.tbl_region on usr.tbl_region_id equals reg.id
                                      join prov in _context.tbl_province on usr.tbl_province_id equals prov.id
                                      join ct in _context.tbl_city on usr.tbl_city_id equals ct.id
                                      join brngy in _context.tbl_brgy on usr.tbl_brgy_id equals brngy.id
                                      where a.tbl_user_id == usid && a.id == applid
                                      select new ApplicantListViewModel
                                      {
                                          id = a.id,
                                          tbl_user_id = usid,
                                          full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                          email = usr.email,
                                          contact = usr.contact_no,
                                          address = usr.street_address,
                                          application_type = appt.name,
                                          permit_type = pT.name,
                                          permit_status = pS.status,
                                          user_type = usrtyps.name,
                                          valid_id = usr.valid_id,
                                          valid_id_no = usr.valid_id_no,
                                          birth_date = usr.birth_date.ToString(),
                                          region = reg.name,
                                          province = prov.name,
                                          city = ct.name,
                                          brgy = brngy.name,
                                          comment = usr.comment
                                      }).FirstOrDefault();
                mymodel.applicantViewModels = applicationMod;

                //Display List of Comments for Application Approval
                mymodel.commentsViewModelsList = (from c in _context.tbl_comments
                                                  where c.tbl_application_id == applid
                                                  join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                  join usr in _context.tbl_user on c.created_by equals usr.id
                                                  select new CommentsViewModel
                                                  {
                                                      tbl_application_id = c.tbl_application_id,
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
        public IActionResult ChainsawOwnerApproval(ViewModel viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();
            int? id = Convert.ToInt32(viewMod.appid);
            int? tbl_user_id = Convert.ToInt32(viewMod.uid);
            if (id == null)
            {
                return View();
            }
            else
            {
                int usid = Convert.ToInt32(tbl_user_id);
                int applid = Convert.ToInt32(id);
                string buttonClicked = viewMod.decision;
                if (buttonClicked == "Approve")
                {
                    //var applicationToUpdate = _context.tbl_application.Find(appID);
                    var appli = new tbl_application() { id = applid, status = 2, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        _context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    var subject = "Chainsaw Owner Permit Application Status";
                    var body = "Greetings! \n We would like to inform you that your Permit Application has been approved.\nThe officer left the following comment:\n" + viewMod.applicantViewModels.comment;
                    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                }
                else if (buttonClicked == "Decline")
                {
                    var appli = new tbl_application() { id = applid, status = 3, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        _context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    var subject = "Chainsaw Owner Permit Application Status";
                    var body = "Greetings! \n We regret to inform you that your Permit Application has been declined.\nThe officer left the following comment:\n" + viewMod.applicantViewModels.comment;
                    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                }
                else
                {
                    var appli = new tbl_application() { id = applid, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
                    if (viewMod.filesUpload != null)
                    {
                        foreach (var file in viewMod.filesUpload.Files)
                        {
                            var filesDB = new tbl_files();
                            FileInfo fileInfo = new FileInfo(file.FileName);
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/UserDocs");

                            //create folder if not exist
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);


                            string fileNameWithPath = Path.Combine(path, file.FileName);

                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            filesDB.tbl_application_id = id;
                            filesDB.created_by = usid;
                            filesDB.modified_by = usid;
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
                    using (_context)
                    {
                        _context.tbl_application.Attach(appli);
                        //_context.Entry(appli).Property(x => x.status).IsModified = true;
                        _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                        _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                        _context.Entry(usrdet).Property(x => x.comment).IsModified = true;
                        _context.SaveChanges();
                    }
                    //Email
                    var subject = "Permit Application Status";
                    var body = "Greetings! \n An inspector viewed your application.\nThe officer left the following comment:\n" + viewMod.applicantViewModels.comment;
                    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                }
                return RedirectToAction("ChainsawOwnerApplicantsList", "ChainsawOwner");
            }

        }

        public IActionResult ChainsawOwnerApplicantsList()
        {
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("Index", "ChainsawOwner");
            }
            else
            {
                ViewModel mymodel = new ViewModel();

                var applicationlist = from a in _context.tbl_application
                                          //where a.tbl_user_id == userID
                                      where a.tbl_application_type_id == 1
                                      select a;

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;

                var applicationMod = from a in applicationlist
                                     join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                     join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                     join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                     join pS in _context.tbl_permit_status on a.status equals pS.id
                                     //where a.tbl_user_id == userID
                                     select new ApplicantListViewModel { id = a.id, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
                //return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult CommentSection(int? uid, int? appid, ViewModel model)
        {
            var commentsTbl = new tbl_comments();
            //commentsTbl.id = model.tbl_Comments.id;
            commentsTbl.tbl_application_id = Convert.ToInt32(appid);
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
                return RedirectToAction("EditApplication", "Application", new { uid = uid, appid = appid });
            }
            else
            {
                return RedirectToAction("ChainsawOwnerApproval", "ChainsawOwner", new { uid = uid, appid = model.appid });
            }
        }
    }
}
