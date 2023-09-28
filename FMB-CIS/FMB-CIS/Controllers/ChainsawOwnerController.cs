
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
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("DENR") == true)
            {
                return RedirectToAction("ChainsawOwnerApplicantsList", "ChainsawOwner");
            }
            else
            {

                return View();
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
                model.tbl_Application.tbl_application_type_id = 2;
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
                    filesDB.tbl_application_id = appID;
                    filesDB.created_by = userID;
                    filesDB.modified_by = userID;
                    filesDB.date_created = DateTime.Now;
                    filesDB.date_modified = DateTime.Now;
                    filesDB.filename = file.FileName;
                    filesDB.path = path;
                    filesDB.tbl_file_type_id = fileInfo.Extension;
                    filesDB.tbl_file_sources_id = fileInfo.Extension;
                    _context.tbl_files.Add(filesDB);
                    _context.SaveChanges();
                }
                //Email
                var subject = "Permit to Import Application Status";
                var body = "Greetings! \n We would like to inform you that your Permit to Import Application has been received.";
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

                var applicationMod = from a in applicationlist
                                     join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                     join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                     join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                     join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                     join pS in _context.tbl_permit_status on a.status equals pS.id
                                     where a.tbl_user_id == usid && a.id == applid
                                     select new ApplicantListViewModel { id = a.id, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, user_type = usrtyps.name, valid_id = usr.valid_id, valid_id_no = usr.valid_id_no, birth_date = usr.birth_date.ToString(), tbl_region_id = usr.tbl_region_id, tbl_province_id = usr.tbl_province_id, tbl_city_id = usr.tbl_city_id, tbl_brgy_id = usr.tbl_brgy_id, comment = usr.comment };

                mymodel.applicantListViewModels = applicationMod;
                //mymodel.tbl_Users = UserInfo;
                return View(mymodel);
            }

        }
        [HttpPost]
        //[Url("?email={email}&code={code}")]
        public IActionResult ChainsawOwnerApproval(int? appID, int? uid, string SubmitButton, ViewModel viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();

            if (appID == null)
            {
                return View();
            }
            else
            {
                int usid = Convert.ToInt32(uid);
                int applid = Convert.ToInt32(appID);
                string buttonClicked = SubmitButton;
                if (buttonClicked == "Approve")
                {
                    //var applicationToUpdate = _context.tbl_application.Find(appID);
                    var appli = new tbl_application() { id = applid, status = 2, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.comment };
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
                    var body = "Greetings! \n We would like to inform you that your Permit Application has been approved.\nThe officer left the following comment:\n" + viewMod.comment;
                    EmailSender.SendEmailAsync(viewMod.email, subject, body);
                }
                else if (buttonClicked == "Decline")
                {
                    var appli = new tbl_application() { id = applid, status = 3, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.comment };
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
                    var body = "Greetings! \n We regret to inform you that your Permit Application has been declined.\nThe officer left the following comment:\n" + viewMod.comment;
                    EmailSender.SendEmailAsync(viewMod.email, subject, body);
                }
                else
                {
                    var appli = new tbl_application() { id = applid, date_modified = DateTime.Now, modified_by = loggedUserID };
                    var usrdet = new tbl_user() { id = usid, comment = viewMod.comment };
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
                    var body = "Greetings! \n An inspector viewed your application.\nThe officer left the following comment:\n" + viewMod.comment;
                    EmailSender.SendEmailAsync(viewMod.email, subject, body);
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
                                     select new ApplicantListViewModel { id = a.id, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = usr.id };

                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
                //return RedirectToAction("Index", "Home");
            }
        }
    }
}
