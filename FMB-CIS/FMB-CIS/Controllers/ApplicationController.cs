using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }


        public ApplicationController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ManageApplications", "Application");
            //return View();

        }

        public IActionResult ManageApplications()
        {

                int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                ViewModel mymodel = new ViewModel();

                var applicationlist = from a in _context.tbl_application
                                      where a.tbl_user_id == userID
                                      //where a.tbl_application_type_id == 3
                                      select a;

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;

                var applicationMod = from a in applicationlist
                                     join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                     join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                     join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                     join pS in _context.tbl_permit_status on a.status equals pS.id
                                     //where a.tbl_user_id == userID
                                     select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
            
        }
        public IActionResult ResellPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Permit to Re-sell/Transfer Ownership"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult LendPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Authority to Lend"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult RegistrationPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Certificate of Registration"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult RentPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Authority to Rent"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult LeasePermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Authority to Lease"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult SellPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Permit to Sell"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }

        public IActionResult PurchasePermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Permit to Purchase"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        public IActionResult ImportPermits()
        {

            int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            ViewModel mymodel = new ViewModel();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == userID
                                  //where a.tbl_application_type_id == 3
                                  select a;

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on a.status equals pS.id
                                 where pT.name == "Permit to Import"
                                 select new ApplicantListViewModel { id = a.id, applicationDate = a.date_created, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = (int)usr.id };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [HttpGet]
        public IActionResult EditApplication(string uid, string appid)
        {
            ViewModel mymodel = new ViewModel();
            //tbl_user user = _context.tbl_user.Find(uid);
            if (uid == null || appid == null)
            {
                ModelState.AddModelError("", "Invalid Importer Application");
                return RedirectToAction("ChainsawImporterApplicantsList", "ChainsawImporter");
            }

            else
            {
                int usid = Convert.ToInt32(uid);
                int applid = Convert.ToInt32(appid);
                var UserList = _context.tbl_user.Single(i => i.id == usid);
                //var UserInfo = UserList.Where(m => m.id == usid).ToList();

                //ViewModel mymodel = new ViewModel();
                var applicationlist = from a in _context.tbl_application
                                      where a.tbl_user_id == usid && a.id == applid
                                      select a;

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

                //HISTORY
                var applicationtypelist = _context.tbl_application_type;
                var applicationMod = (from a in applicationlist
                                      join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                      join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                      join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                      join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                      join pS in _context.tbl_permit_status on a.status equals pS.id
                                      where a.tbl_user_id == usid && a.id == applid
                                      select new ApplicantListViewModel
                                      {
                                          id = a.id,
                                          tbl_user_id = usid,
                                          full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                          email = usr.email,
                                          permit_type = pT.name,
                                          permit_status = pS.status,
                                          user_type = usrtyps.name,
                                          comment = usr.comment,
                                          qty = a.qty,
                                          specification = a.tbl_specification_id,
                                          inspectionDate = a.date_of_inspection,
                                          address = a.supplier_address,
                                          expectedTimeArrived = a.expected_time_arrival,
                                          expectedTimeRelease = a.expected_time_release,
                                          purpose = a.purpose
                                      }).FirstOrDefault();
                //mymodel.email = UserList.email;
                mymodel.applicantViewModels = applicationMod;
                //mymodel.tbl_Users = UserInfo;

                //Display List of Comments for Application Approval (User to Inspector Conversation)
                mymodel.commentsViewModelsList = (from c in _context.tbl_comments
                                                  where c.tbl_application_id == applid
                                                  //join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                  join usr in _context.tbl_user on c.created_by equals usr.id
                                                  select new CommentsViewModel
                                                  {
                                                      tbl_application_id = c.tbl_application_id,
                                                      //tbl_files_id = c.tbl_files_id,
                                                      //fileName = f.filename,
                                                      comment_to = c.comment_to,
                                                      comment = c.comment,
                                                      commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                      created_by = c.created_by,
                                                      modified_by = c.modified_by,
                                                      date_created = c.date_created,
                                                      date_modified = c.date_modified
                                                  }).Where(u => u.comment_to == "User To Inspector").OrderByDescending(d => d.date_created);

                //Display List of Comments for Application Approval (User to CENRO Conversation)
                mymodel.commentsViewModels2ndList = (from c in _context.tbl_comments
                                                  where c.tbl_application_id == applid
                                                  //join f in _context.tbl_files on c.tbl_files_id equals f.Id
                                                  join usr in _context.tbl_user on c.created_by equals usr.id
                                                  select new CommentsViewModel
                                                  {
                                                      tbl_application_id = c.tbl_application_id,
                                                      //tbl_files_id = c.tbl_files_id,
                                                      //fileName = f.filename,
                                                      comment_to = c.comment_to,
                                                      comment = c.comment,
                                                      commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
                                                      created_by = c.created_by,
                                                      modified_by = c.modified_by,
                                                      date_created = c.date_created,
                                                      date_modified = c.date_modified
                                                  }).Where(u => u.comment_to == "User To CENRO").OrderByDescending(d => d.date_created);

                return View(mymodel);
            }
        }

        [HttpPost]
        public IActionResult EditApplication(ViewModel? viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            int usid = Convert.ToInt32(viewMod.applicantViewModels.tbl_user_id);
            int applid = Convert.ToInt32(viewMod.applicantViewModels.id);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();


            if (viewMod.applicantViewModels.id != null)
            {

                //string buttonClicked = SubmitButton;

                //updating the application
                var appliDB = _context.tbl_application.Where(m => m.id == viewMod.applicantViewModels.id).FirstOrDefault();
                
                //appliDB.id = applid;
                appliDB.qty = viewMod.applicantViewModels.qty;
                appliDB.purpose = viewMod.applicantViewModels.purpose;
                appliDB.expected_time_arrival = viewMod.applicantViewModels.expectedTimeArrived;
                appliDB.expected_time_release = viewMod.applicantViewModels.expectedTimeRelease;
                appliDB.date_modified = DateTime.Now;
                appliDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
                appliDB.supplier_address = viewMod.applicantViewModels.address;
                appliDB.date_of_inspection = viewMod.applicantViewModels.inspectionDate;
                appliDB.tbl_specification_id = viewMod.applicantViewModels.specification;

                
                _context.SaveChanges();
                

                //Saving a file
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
                            filesDB.tbl_application_id = viewMod.applicantViewModels.id;
                            filesDB.created_by = viewMod.applicantViewModels.tbl_user_id;
                            filesDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
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
                    var body = "Greetings! \n You have successfully edited your application.";
                    EmailSender.SendEmailAsync(viewMod.applicantViewModels.email, subject, body);
                
                
                
            }

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == usid && a.id == applid
                                  select a;
            //CODE FOR FILE DOWNLOAD
            int applicID = Convert.ToInt32(viewMod.applicantViewModels.id);
            //File Paths from Database
            var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID).ToList();
            List<tbl_files> files = new List<tbl_files>();

            foreach (var fileList in filesFromDB)
            {
                files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                //files.Add(new tbl_files { filename = f });
            }

            viewMod.tbl_Files = files;
            //END FOR FILE DOWNLOAD

            //HISTORY
            var applicationtypelist = _context.tbl_application_type;
            var applicationMod = (from a in applicationlist
                                  join usr in _context.tbl_user on a.tbl_user_id equals usr.id
                                  join usrtyps in _context.tbl_user_types on usr.tbl_user_types_id equals usrtyps.id
                                  join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                  join pT in _context.tbl_permit_type on a.tbl_permit_type_id equals pT.id
                                  join pS in _context.tbl_permit_status on a.status equals pS.id
                                  where a.tbl_user_id == usid && a.id == applid
                                  select new ApplicantListViewModel
                                  {
                                      id = a.id,
                                      tbl_user_id = usid,
                                      full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                      email = usr.email,
                                      permit_type = pT.name,
                                      permit_status = pS.status,
                                      user_type = usrtyps.name,
                                      comment = usr.comment,
                                      qty = a.qty,
                                      specification = a.tbl_specification_id,
                                      inspectionDate = a.date_of_inspection,
                                      address = a.supplier_address,
                                      expectedTimeArrived = a.expected_time_arrival,
                                      expectedTimeRelease = a.expected_time_release,
                                      purpose = a.purpose
                                  }).FirstOrDefault();

            ViewBag.Message = "Save Success";
            viewMod.applicantViewModels = applicationMod;

            return View(viewMod);

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
        public IActionResult CommentSection(int? uid, int? appid, ViewModel model)
        {
            var commentsTbl = new tbl_comments();
            //commentsTbl.id = model.tbl_Comments.id;
            commentsTbl.tbl_application_id = Convert.ToInt32(appid);
            //commentsTbl.tbl_files_id = model.tbl_Comments.tbl_files_id;
            commentsTbl.comment_to = model.tbl_Comments.comment_to;
            commentsTbl.comment = model.tbl_Comments.comment;
            commentsTbl.created_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.modified_by = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            commentsTbl.date_created = DateTime.Now;
            commentsTbl.date_modified = DateTime.Now;
            _context.tbl_comments.Add(commentsTbl);
            _context.SaveChanges();
            //return RedirectToAction("AccountsApproval?uid="+uid, "AccountManagement");
            //Url.Action("A","B",new{a="x"})

            
            return RedirectToAction("EditApplication", "Application", new { uid = uid, appid = appid });
        }
    }
}
