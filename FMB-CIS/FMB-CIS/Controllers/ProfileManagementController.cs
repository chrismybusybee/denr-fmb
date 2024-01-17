using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class ProfileManagementController : Controller
    {
        private readonly LocalContext _context;
        private readonly IConfiguration _configuration;
        private IEmailSender EmailSender { get; set; }
        private IWebHostEnvironment EnvironmentWebHost;

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
            //bool? status = usInfo.status;
            ViewModel model = new ViewModel();


            var currentUser = _context.tbl_user.Find(uid);
            model.tbl_User = currentUser;
            //model.tbl_User.id = uid;

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

            //Get list of required documents from tbl_announcement
            //var requirements = _context.tbl_announcement.Where(a => a.id == 1).FirstOrDefault();
            //model.soloAnnouncement = requirements;
            //End for required documents

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
            //File Paths from Database
            //var filesFromDB = _context.tbl_files.Where(f => f.tbl_user_id == uid && !f.path.Contains("UserPhotos")).ToList();
            //List<tbl_files> files = new List<tbl_files>();

            //foreach (var fileList in filesFromDB)
            //{
            //    files.Add(new tbl_files { Id = fileList.Id, filename = fileList.filename, path = fileList.path, tbl_file_type_id = fileList.tbl_file_type_id, date_created = fileList.date_created, file_size = fileList.file_size });
            //}

            //model.tbl_Files = files;
            //END FOR FILE DOWNLOAD

            //Profile Photo Source
            //bool profilePhotoExist = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).Any();
            //if (profilePhotoExist == true)
            //{
            //    var profilePhoto = _context.tbl_files.Where(f => f.tbl_user_id == uid && f.path.Contains("UserPhotos") && f.is_active == true).FirstOrDefault();
            //    ViewBag.profilePhotoSource = "/Files/UserPhotos/" + profilePhoto.filename;
            //}
            //else
            //{
            //    ViewBag.profilePhotoSource = "/assets/images/default-avatar.png";
            //}
            //END for Profile Photo Source

            //Display List of Comments
            //model.commentsViewModelsList = (from c in _context.tbl_comments
            //                                where c.tbl_user_id == uid
            //                                join f in _context.tbl_files on c.tbl_files_id equals f.Id
            //                                join usr in _context.tbl_user on c.created_by equals usr.id
            //                                select new CommentsViewModel
            //                                {
            //                                    tbl_user_id = c.tbl_user_id,
            //                                    tbl_files_id = c.tbl_files_id,
            //                                    fileName = f.filename,
            //                                    comment = c.comment,
            //                                    commenterName = usr.first_name + " " + usr.last_name + " " + usr.suffix,
            //                                    created_by = c.created_by,
            //                                    modified_by = c.modified_by,
            //                                    date_created = c.date_created,
            //                                    date_modified = c.date_modified
            //                                }).OrderBy(f => f.fileName).ThenByDescending(d => d.date_created);
            return View(model);
            //return View();
        }
    }
}
