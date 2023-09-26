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

namespace FMB_CIS.Controllers
{
    [Authorize]
    public class ChainsawOwnerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LocalContext _context;

        public ChainsawOwnerController(IConfiguration configuration, LocalContext context)
        {
            this._configuration = configuration;
            _context = context;
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
        public IActionResult Index(OwnerPermitViewModel model)
        {
            //try
            //{
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
                //DAL dal = new DAL();

                //SAVE USER INFOS ON DATABASE USING CreateNewUserNoPHOTO STORED PROCEDURE
                int one = 1;
                int two = 2;
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("ConnStrng")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("OwnerPermitsRegistration", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("tbl_user_id", userID);
                    sqlCmd.Parameters.AddWithValue("tbl_application_type_id", one); //1 - Certificate of registration, 2 - Chainsaw Importer, 3 - Chainsaw Seller
                    sqlCmd.Parameters.AddWithValue("tbl_permit_type_id", Convert.ToInt32(model.tbl_permit_type_id)); //1 - Permit to Import, 2 - Permit to Purchase, 3 - Permit to Sell, 4 - Transfer of Ownership, 5 - Authority to Lease, 6 - Authority to Rent, 7 - Authority to Lend
                    sqlCmd.Parameters.AddWithValue("supplier_fname", model.supplier_fname);
                    sqlCmd.Parameters.AddWithValue("supplier_mname", model.supplier_mname);
                    sqlCmd.Parameters.AddWithValue("supplier_lname", model.supplier_lname);
                    sqlCmd.Parameters.AddWithValue("supplier_suffix", model.supplier_suffix ?? "");
                    sqlCmd.Parameters.AddWithValue("supplier_contact_no", model.supplier_contact_no);
                    sqlCmd.Parameters.AddWithValue("supplier_address", model.supplier_address);
                    sqlCmd.Parameters.AddWithValue("supplier_email", model.supplier_email);
                    sqlCmd.Parameters.AddWithValue("qty", model.qty);
                    //SPECIFICATION
                    sqlCmd.Parameters.AddWithValue("tbl_specification_id", model.tbl_specification_id);
                    sqlCmd.Parameters.AddWithValue("purpose", model.purpose);
                    sqlCmd.Parameters.AddWithValue("expected_time_arrival", model.expected_time_arrival.ToString());
                    sqlCmd.Parameters.AddWithValue("expected_time_release", model.expected_time_release.ToString());
                    sqlCmd.Parameters.AddWithValue("date_of_inspection", model.date_of_inspection.ToString());
                    sqlCmd.Parameters.AddWithValue("is_active", one);
                    sqlCmd.Parameters.AddWithValue("created_by", userID);
                    sqlCmd.Parameters.AddWithValue("modified_by", userID);
                    sqlCmd.Parameters.AddWithValue("date_created", DateTime.Now);
                    sqlCmd.Parameters.AddWithValue("date_modified", DateTime.Now);
                    sqlCmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index", "Home");
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
                }
                else
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
