using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserRegistrationViewModel
    {
        /*
        public int id;
        //[Required]
        [Display (Name ="First Name")]
        public string first_name { get; set; }
        [Display (Name ="Middle Name")]
        public string middle_name { get; set; }
        [Display (Name ="Last Name")]
        public string last_name { get; set; }
        [Display (Name ="Suffix")]
        public string? suffix { get; set; }
        [Display (Name ="Contact Number")]
        public string contact_no { get; set; }
        [Display (Name ="Valid ID")]
        public string valid_id { get; set; }
        [Display (Name ="Valid ID Number")]
        public string valid_id_no { get; set; }
        [Display (Name ="Birth Date")]
        public DateTime birth_date { get; set; }
        [Display(Name = "Region")]
        public int tbl_region_id { get; set; }
        [Display(Name = "Province")]
        public int tbl_province_id { get; set; }
        //public string tbl_province_name { get; set; }
        [Display(Name = "City")]
        public int tbl_city_id { get; set; }
        //public string tbl_city_name { get; set; }
        [Display(Name = "Barangay")]
        public int tbl_brgy_id { get; set; }
        //public string tbl_brgy_name { get; set; }
        public string street_address { get; set; }
        public string? tbl_division_id { get; set; }
        [Display (Name ="Email Address")]
        public string email { get; set; }
        //[DataType(DataType.Password)]
        //[Required]
        //[Display (Name ="Password")]
        //public string password { get; set; }
        //[DataType(DataType.Password)]
        //[Display (Name ="Confirm Password")]
        //public string confirmPassword { get; set; }
        //public string status { get; set; }
        //public byte[] photo { get; set; }
        //public string photo { get; set; }
        [Display (Name ="Remarks")]
        public string? comment { get; set; }
        [Required]
        [Display (Name ="User Type")]
        public string tbl_user_types_id { get; set; }
        //public DateTime date_created { get; set; }

        //public DateTime date_modified { get; set; }
        */

        public IEnumerable<tbl_region>? tbl_Regions { get; set; }
        public IEnumerable<tbl_province>? tbl_Provinces { get; set; }
        public IEnumerable<tbl_city>? tbl_Cities { get; set; }
        public IEnumerable<tbl_brgy>? tbl_Brgys { get; set; }
        public IEnumerable<tbl_files>? tbl_Files { get; set; }
        public FileUpload? filesUpload { get; set; }
        public tbl_user? tbl_Users { get; set; }
        public RECaptcha? reCaptcha { get; set;}
        //[Required]
        //public string Token { get; set; }
        public tbl_user_temp_passwords? tbl_User_Temp_Passwords { get; set; }
        //public FileUpload? profilePicUpload { get; set; }
        public tbl_announcement? soloAnnouncement { get; set;}
    }
}
