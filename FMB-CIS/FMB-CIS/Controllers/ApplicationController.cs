using FMB_CIS.Data;
using FMB_CIS.Models;
using Humanizer.Localisation;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using Org.BouncyCastle.Tls;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Security.Claims;
using System.Security.Cryptography.Xml;

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
                                 select new ApplicantListViewModel 
                                 {
                                     id = a.id,
                                     applicationDate = a.date_created,
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name,
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 {
                                     id = a.id,
                                     applicationDate = a.date_created, 
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, 
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name, 
                                     permit_status = pS.status, 
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 { 
                                     id = a.id,
                                     applicationDate = a.date_created,
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name, 
                                     permit_type = pT.name,
                                     permit_status = pS.status, 
                                     tbl_user_id = (int)usr.id,
                                     date_of_expiration = a.date_of_expiration,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 { 
                                     id = a.id, 
                                     applicationDate = a.date_created, 
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email, 
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name, 
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 {
                                     id = a.id,
                                     applicationDate = a.date_created,
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name, 
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 { 
                                     id = a.id,
                                     applicationDate = a.date_created,
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address, 
                                     application_type = appt.name, 
                                     permit_type = pT.name,
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 {
                                     id = a.id,
                                     applicationDate = a.date_created, 
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix, 
                                     email = usr.email, 
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name, 
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     qty = a.qty
                                 };

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
                                 select new ApplicantListViewModel 
                                 { 
                                     id = a.id, 
                                     applicationDate = a.date_created,
                                     full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                     email = usr.email,
                                     contact = usr.contact_no,
                                     address = usr.street_address,
                                     application_type = appt.name,
                                     permit_type = pT.name,
                                     permit_status = pS.status,
                                     tbl_user_id = (int)usr.id,
                                     status=(int)a.status,
                                     qty = a.qty
                                 };

            mymodel.applicantListViewModels = applicationMod;

            return View(mymodel);

        }
        [HttpGet]
        public IActionResult EditApplication(string uid, string appid)
        {
            ViewModel mymodel = new ViewModel();
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //tbl_user user = _context.tbl_user.Find(uid);
            if (uid == null || appid == null || loggedUserID != Convert.ToInt32(uid))
            {
                ModelState.AddModelError("", "Invalid Application");
                return RedirectToAction("Index", "Dashboard");
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
                var filesFromDB = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == usid && f.is_proof_of_payment != true).ToList();
                List<tbl_files> files = new List<tbl_files>();

                foreach (var fileList in filesFromDB)
                {
                    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                    //files.Add(new tbl_files { filename = f });
                }
                mymodel.tbl_Files = files;

                //FILES FOR PROOF OF PAYMENT
                var filesFromPayment = _context.tbl_files.Where(f => f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && f.is_proof_of_payment == true).ToList();
                List<tbl_files> paymentFiles = new List<tbl_files>();

                foreach (var fileList in filesFromPayment)
                {
                    paymentFiles.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, file_size = fileList.file_size, date_created = fileList.date_created });
                }
                mymodel.proofOfPaymentFiles = paymentFiles;
                //END FOR FILE DOWNLOAD

                //Document Tagging
                //var fileWithCommentsforDocTagging = (from f in _context.tbl_files
                //                                    //join c in _context.tbl_comments on f.Id equals c.tbl_files_id
                //                                    //join usr in _context.tbl_user on c.created_by equals usr.id
                //                                    where f.tbl_application_id == applicID && f.created_by == usid
                //                                    select new FilesWithComments
                //                                    {
                //                                        tbl_files_id = f.Id,
                //                                        filename = f.filename,
                //                                        tbl_application_id = f.tbl_application_id,
                //                                        tbl_files_status = f.status,
                //                                        //comment = c.comment
                //                                    }).ToList();

                ////Add the latest comments for every file
                //var commentsList = _context.tbl_comments.Where(c => c.tbl_application_id == applicID).ToList();

                //for (int i=0; i< fileWithCommentsforDocTagging.Count; i++)
                //{
                //    var latestComment = commentsList.Where(c => c.tbl_files_id == fileWithCommentsforDocTagging[i].tbl_files_id).LastOrDefault();
                //    if (latestComment != null)
                //    {
                //        fileWithCommentsforDocTagging[i].comment = latestComment.comment;
                //        fileWithCommentsforDocTagging[i].tbl_comments_created_by = latestComment.created_by;
                //    }
                //}

                //mymodel.filesWithComments = fileWithCommentsforDocTagging;
                //End for Document Tagging

                //Get application permit type id
                int permitTypeID = Convert.ToInt32(_context.tbl_application.Where(a => a.id == applicID).Select(a => a.tbl_permit_type_id).FirstOrDefault());
                //Document Tagging and Checklist
                //Get uploaded files and requirements
                var fileWithCommentsforDocTagging = (from dc in _context.tbl_document_checklist
                                                     join f in _context.tbl_files on dc.id equals f.checklist_id
                                                     where dc.permit_type_id == permitTypeID && f.tbl_application_id == applicID && f.created_by == Convert.ToInt32(uid) && dc.is_active == true
                                                     select new FilesWithComments
                                                     {
                                                         tbl_document_checklist_id = dc.id,
                                                         tbl_document_checklist_name = dc.name,
                                                         tbl_files_id = f.Id,
                                                         filename = f.filename,
                                                         tbl_application_id = f.tbl_application_id,
                                                         tbl_files_status = f.status
                                                         //comment = c.comment
                                                     }).ToList();

                var requiredDocumentList = _context.tbl_document_checklist.Where(c => c.permit_type_id == permitTypeID && c.is_active == true).ToList();
                foreach (var reqList in requiredDocumentList)
                {
                    bool isReqAvailable = fileWithCommentsforDocTagging.Any(r => r.tbl_document_checklist_id == reqList.id);
                    if (isReqAvailable == false)
                    {
                        fileWithCommentsforDocTagging.Add(new FilesWithComments
                        {
                            tbl_document_checklist_id = reqList.id,
                            tbl_document_checklist_name = reqList.name,
                            tbl_files_id = null,
                            filename = "N/A",
                            tbl_application_id = applicID,
                            tbl_files_status = "N/A"
                        });
                    }
                }

                //Add the latest comments for every file
                var commentsList = _context.tbl_comments.Where(c => c.tbl_application_id == applicID).ToList();

                for (int i = 0; i < fileWithCommentsforDocTagging.Count; i++)
                {
                    var latestComment = commentsList.Where(c => c.tbl_files_id == fileWithCommentsforDocTagging[i].tbl_files_id).LastOrDefault();
                    if (latestComment != null)
                    {
                        fileWithCommentsforDocTagging[i].comment = latestComment.comment;
                        fileWithCommentsforDocTagging[i].tbl_comments_created_by = latestComment.created_by;
                    }
                }

                mymodel.filesWithComments = fileWithCommentsforDocTagging;
                //End for Document Tagging and Checklist

                var permitTypeOfThisApplication  = _context.tbl_application.Where(a => a.id == applid).Select(a => a.tbl_permit_type_id).FirstOrDefault();
                if(permitTypeOfThisApplication == 13) //For Certificate of Registration
                {
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
                                          join csaw in _context.tbl_chainsaw on a.id equals csaw.tbl_application_id
                                          where a.tbl_user_id == usid && a.id == applid
                                          select new ApplicantListViewModel
                                          {
                                              id = a.id,
                                              tbl_user_id = usid,
                                              full_name = usr.first_name + " " + usr.middle_name + " " + usr.last_name + " " + usr.suffix,
                                              full_address = usr.street_address + " " + brngy.name + " " + ct.name + " " + prov.name + " " + reg.name,
                                              email = usr.email,
                                              permit_type = pT.name,
                                              permit_status = pS.status,
                                              status = Convert.ToInt32(a.status),
                                              user_type = usrtyps.name,
                                              comment = usr.comment,
                                              qty = a.qty,
                                              specification = a.tbl_specification_id,
                                              initial_date_of_inspection = a.initial_date_of_inspection,
                                              inspectionDate = a.date_of_inspection,
                                              address = usr.street_address,
                                              //expectedTimeArrived = a.expected_time_arrival,
                                              //expectedTimeRelease = a.expected_time_release,
                                              purpose = a.purpose,
                                              date_of_registration = a.date_of_registration,
                                              date_of_expiration = a.date_of_expiration,
                                              chainsawBrand = csaw.Brand,
                                              chainsawModel = csaw.Model,
                                              Engine = csaw.Engine,
                                              powerSource = csaw.Power,
                                              Watt = csaw.watt,
                                              hp = csaw.hp,
                                              gb = csaw.gb,
                                              chainsaw_serial_number = csaw.chainsaw_serial_number,
                                              chainsawSupplier = csaw.supplier,
                                              date_purchase = csaw.date_purchase,
                                          }).FirstOrDefault();

                    mymodel.applicantViewModels = applicationMod;
                }
                else
                {
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
                                              full_address = usr.street_address + " " + brngy.name + " " + ct.name + " " + prov.name + " " + reg.name,
                                              email = usr.email,
                                              permit_type = pT.name,
                                              permit_status = pS.status,
                                              status = Convert.ToInt32(a.status),
                                              user_type = usrtyps.name,
                                              comment = usr.comment,
                                              qty = a.qty,
                                              specification = a.tbl_specification_id,
                                              initial_date_of_inspection = a.initial_date_of_inspection,
                                              inspectionDate = a.date_of_inspection,
                                              address = usr.street_address,
                                              //expectedTimeArrived = a.expected_time_arrival,
                                              //expectedTimeRelease = a.expected_time_release,
                                              purpose = a.purpose,
                                              date_of_registration = a.date_of_registration,
                                              date_of_expiration = a.date_of_expiration,
                                              //coordinatedWithEnforcementDivision = a.coordinatedWithEnforcementDivision
                                          }).FirstOrDefault();
                    //mymodel.email = UserList.email;
                    mymodel.applicantViewModels = applicationMod;
                    //mymodel.tbl_Users = UserInfo;
                }


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
                
                //Set the value for announcementID (used to display the required documents depending on permit type)
                int announcementID = 0;
                var applicationInfo = _context.tbl_application.Where(a => a.id == applid).FirstOrDefault();
                switch(applicationInfo.tbl_permit_type_id)
                {
                    
                    case 1: //1   Permit to Import
                        announcementID = 2; //2   Permit to Import Requirements
                        break;
                    case 2: //2   Permit to Purchase
                        announcementID = 3; //3   Permit to Purchase Requirements
                        break;
                    case 3: //3   Permit to Sell
                        announcementID = 4; //4   Permit to Sell Requirements
                        break;
                    case 4: //4   Transfer of Ownership
                        announcementID = 7; //7   Transfer of Ownership Requirements
                        break;
                    case 5: //5   Authority to Lease
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 6: //6   Authority to Rent
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 7: //7   Authority to Lend
                        announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                        break;
                    case 13: //13  Certificate of Registration
                        announcementID = 5; //5   Certificate of Registration Requirements
                        break;
                    case 14: //14  Permit to Re - sell / Transfer Ownership
                        announcementID = 7; //7   Transfer of Ownership Requirements
                        break;
                }
                //Get list of required documents from tbl_announcement
                var requirements = _context.tbl_announcement.Where(a => a.id == announcementID).FirstOrDefault();
                ViewBag.RequiredDocsList = requirements.announcement_content;
                //End for required documents
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
                //appliDB.expected_time_arrival = viewMod.applicantViewModels.expectedTimeArrived;
                //appliDB.expected_time_release = viewMod.applicantViewModels.expectedTimeRelease;
                appliDB.date_modified = DateTime.Now;
                appliDB.modified_by = viewMod.applicantViewModels.tbl_user_id;
                appliDB.supplier_address = viewMod.applicantViewModels.address;
                appliDB.date_of_inspection = viewMod.applicantViewModels.inspectionDate;
                appliDB.tbl_specification_id = viewMod.applicantViewModels.specification;
                //if (appliDB.tbl_permit_type_id == 2 || appliDB.tbl_permit_type_id == 3) //For Permit to Purchase and Permit to Sell
                //{
                //    appliDB.coordinatedWithEnforcementDivision = viewMod.applicantViewModels.coordinatedWithEnforcementDivision;
                //}
                _context.SaveChanges();

                if(appliDB.tbl_permit_type_id == 13)
                {
                    var csawDB = _context.tbl_chainsaw.Where(c => c.tbl_application_id == appliDB.id).FirstOrDefault();
                    csawDB.Brand = viewMod.applicantViewModels.chainsawBrand;
                    csawDB.Model = viewMod.applicantViewModels.chainsawModel;
                    csawDB.Engine = viewMod.applicantViewModels.Engine;
                    csawDB.Power = viewMod.applicantViewModels.powerSource;
                    if (viewMod.applicantViewModels.powerSource == "Gas")
                    {
                        csawDB.hp = viewMod.applicantViewModels.hp;
                        csawDB.watt = null;
                    }
                    else
                    {
                        csawDB.watt = viewMod.applicantViewModels.Watt;
                        csawDB.hp = null;
                    }                    
                    csawDB.gb = viewMod.applicantViewModels.gb;
                    csawDB.chainsaw_serial_number = viewMod.applicantViewModels.chainsaw_serial_number;
                    csawDB.supplier = viewMod.applicantViewModels.chainsawSupplier;
                    csawDB.date_purchase = viewMod.applicantViewModels.date_purchase;
                    _context.SaveChanges();
                }
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
                                      //expectedTimeArrived = a.expected_time_arrival,
                                      //expectedTimeRelease = a.expected_time_release,
                                      purpose = a.purpose
                                  }).FirstOrDefault();
            
            //Set the value for announcementID (used to display the required documents depending on permit type)
            int announcementID = 0;
            var applicationInfo = _context.tbl_application.Where(a => a.id == applid).FirstOrDefault();
            switch (applicationInfo.tbl_permit_type_id)
            {

                case 1: //1   Permit to Import
                    announcementID = 2; //2   Permit to Import Requirements
                    break;
                case 2: //2   Permit to Purchase
                    announcementID = 3; //3   Permit to Purchase Requirements
                    break;
                case 3: //3   Permit to Sell
                    announcementID = 4; //4   Permit to Sell Requirements
                    break;
                case 4: //4   Transfer of Ownership
                    announcementID = 7; //7   Transfer of Ownership Requirements
                    break;
                case 5: //5   Authority to Lease
                    announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                    break;
                case 6: //6   Authority to Rent
                    announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                    break;
                case 7: //7   Authority to Lend
                    announcementID = 6; //6   Permit to Lease / Rent / Lend Requirements
                    break;
                case 13: //13  Certificate of Registration
                    announcementID = 5; //5   Certificate of Registration Requirements
                    break;
                case 14: //14  Permit to Re - sell / Transfer Ownership
                    announcementID = 7; //7   Transfer of Ownership Requirements
                    break;
            }
            //Get list of required documents from tbl_announcement
            var requirements = _context.tbl_announcement.Where(a => a.id == announcementID).FirstOrDefault();
            ViewBag.RequiredDocsList = requirements.announcement_content;
            //End for required documents
            ViewBag.Message = "Save Success";
            viewMod.applicantViewModels = applicationMod;

            //return View(viewMod);
            return RedirectToAction("EditApplication", "Application", new { uid = usid, appid = applid });
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

        [HttpPost]
        public IActionResult UploadProofOfPayment(ViewModel model, string actionName)
        {
            int applicationID = Convert.ToInt32(model.tbl_Application.id);
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            //var action = ViewContext.RouteData.Values["action"].ToString();
            //var action = ViewBag.ActionName;
            
            //Saving a file
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
                    filesDB.tbl_application_id = applicationID;
                    filesDB.created_by = loggedUserID;
                    filesDB.modified_by = loggedUserID;
                    filesDB.date_created = DateTime.Now;
                    filesDB.date_modified = DateTime.Now;
                    filesDB.filename = file.FileName;
                    filesDB.path = path;
                    filesDB.tbl_file_type_id = fileInfo.Extension;
                    filesDB.tbl_file_sources_id = fileInfo.Extension;
                    filesDB.is_proof_of_payment = true;
                    filesDB.file_size = Convert.ToInt32(file.Length);
                    _context.tbl_files.Add(filesDB);
                    _context.SaveChanges();
                }
            }

            //modify permit status
            int stats = 7; // 7 - Payment Verification (Inspector)
            //SAVE CHANGES TO DATABASE
            DateTime? dateDueOfficer = BusinessDays.AddBusinessDays(DateTime.Now, 2).AddHours(4).AddMinutes(30);
            var appli = new tbl_application() { id = applicationID, status = stats, date_modified = DateTime.Now, modified_by = loggedUserID, date_due_for_officers = dateDueOfficer };
            //var usrdet = new tbl_user() { id = usid, comment = viewMod.applicantViewModels.comment };
            using (_context)
            {
                _context.tbl_application.Attach(appli);
                _context.Entry(appli).Property(x => x.status).IsModified = true;
                _context.Entry(appli).Property(x => x.modified_by).IsModified = true;
                _context.Entry(appli).Property(x => x.date_modified).IsModified = true;
                _context.Entry(appli).Property(x => x.date_due_for_officers).IsModified = true;
                _context.SaveChanges();
            }

            //return View();
            //return new EmptyResult();
            return RedirectToAction(actionName, "Application");
        }
    }
}
