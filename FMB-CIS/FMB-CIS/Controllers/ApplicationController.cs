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
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
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
                                     select new ApplicantListViewModel { id = a.id, full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = usr.id };

                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
            }
            else
            {
                int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                ViewModel mymodel = new ViewModel();

                var applicationlist = from a in _context.tbl_application
                                      //where a.tbl_user_id == userID
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
                                     select new ApplicantListViewModel { id = a.id, 
                                         full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, email = usr.email, contact = usr.contact_no, address = usr.street_address, application_type = appt.name, permit_type = pT.name, permit_status = pS.status, tbl_user_id = usr.id };

                mymodel.applicantListViewModels = applicationMod;

                return View(mymodel);
                //return RedirectToAction("Index", "Home");
            }

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
                //mymodel.applicantListViewModels = applicationMod;
                //mymodel.tbl_Users = UserInfo;
                return View(applicationMod);
            }
        }

        [HttpPost]
        public IActionResult EditApplication(ApplicantListViewModel? viewMod)
        {
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);

            //viewMod.applicantListViewModels.FirstOrDefault(x=>x.comment)
            //string newComment = viewMod.applicantListViewModels.Where(x => x.tbl_user_id == uid).Select(v => v.comment).ToList().ToString();
            
            
            if(viewMod.id != null)
            {
                int usid = Convert.ToInt32(viewMod.tbl_user_id);
                int applid = Convert.ToInt32(viewMod.id);
                //string buttonClicked = SubmitButton;

                //updating the application
                var appliDB = _context.tbl_application.Where(m => m.id == applid).FirstOrDefault();
                
                //appliDB.id = applid;
                appliDB.qty = viewMod.qty;
                appliDB.purpose = viewMod.purpose;
                appliDB.expected_time_arrival = viewMod.expectedTimeArrived;
                appliDB.expected_time_release = viewMod.expectedTimeRelease;
                appliDB.date_modified = DateTime.Now;
                appliDB.modified_by = usid;
                appliDB.supplier_address = viewMod.address;
                appliDB.date_of_inspection = viewMod.inspectionDate;
                appliDB.tbl_specification_id = viewMod.specification;

                
                _context.SaveChanges();
                

                //Saving a file
                if (viewMod.filesUpload != null)
                    {
                        foreach (var file in viewMod.filesUpload.Files)
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
                            filesDB.tbl_application_id = viewMod.id;
                            filesDB.created_by = usid;
                            filesDB.modified_by = usid;
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
                    var subject = "Permit Application Status";
                    var body = "Greetings! \n An inspector viewed your application.\nThe officer left the following comment:\n" + viewMod.comment;
                    EmailSender.SendEmailAsync(viewMod.email, subject, body);
                
                
                
            }
            ViewBag.Message = "Save Success";
            return View(viewMod);
        }
    }
}
