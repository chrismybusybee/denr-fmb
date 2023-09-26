using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class PermitToImportApprovalViewModel
    {
        [Display(Name = "Status")]
        public int status { get; set; }
        [Display(Name = "User Type")]
        public string tbl_user_types_id { get; set; }
        [Display(Name = "Name")]
        public string full_name { get; set; }
        [Display(Name = "Email Address")]
        public string email { get; set; }
        [Display(Name = "Contact Number")]
        public string contact_no { get; set; }
        [Display(Name = "Valid ID")]
        public string valid_id { get; set; }
        [Display(Name = "Valid ID Number")]
        public string valid_id_no { get; set; }
        [Display(Name = "Birth Date")]
        public DateTime birth_date { get; set; }
        [Display(Name = "Street Address")]
        public string street_address { get; set; }
        [Display(Name = "Region")]
        public int tbl_region_id { get; set; }
        [Display(Name = "Province")]
        public int tbl_province_id { get; set; }
        [Display(Name = "City")]
        public int tbl_city_id { get; set; }
        [Display(Name = "Barangay")]
        public int tbl_brgy_id { get; set; }
        [Display(Name = "Barangay")]
        public int comments { get; set; }
    }
}
