using System.ComponentModel.DataAnnotations;
namespace FMB_CIS.Models
{
    public class PermitToImportApplicationViewModel
    {
        //public int id { get; set; }
        //public int tbl_user_id { get; set; }
        //public int tbl_application_type_id { get; set; }
        //public int tbl_permit_type_id { get; set; }
        [Display(Name = "First Name")]
        public string supplier_fname { get; set; }
        [Display(Name = "Middle Name")]
        public string supplier_mname { get; set; }
        [Display(Name = "Last Name")]
        public string supplier_lname { get; set; }
        [Display(Name = "Suffix")]
        public string? supplier_suffix { get; set; }
        [Display(Name = "Contact No.")]
        public string supplier_contact_no { get; set; }
        [Display(Name = "Address")]
        public string supplier_address { get; set; }
        [Display(Name = "Email")]
        public string supplier_email { get; set; }
        [Display(Name = "Quantity")]
        public int qty { get; set; }
        [Display(Name = "Specification ID")]
        public int tbl_specification_id { get; set; }
        //public string specification { get; set; }
        public string purpose { get; set; }
        [Display(Name = "Expected Date of Arrival")]
        public DateTime expected_time_arrival { get; set; }
        [Display(Name = "Expected Date of Release")]
        public DateTime expected_time_release { get; set; }
        [Display(Name = "Date of Inspection")]
        public DateTime date_of_inspection { get; set; }
    }
}
