using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace FMB_CIS.Models
{
    public class tbl_application
    {

        public int? id { get; set; }
        public int? tbl_user_id { get; set; }
        public int? tbl_application_type_id { get; set; }
        public int? tbl_permit_type_id { get; set; }
        public string? supplier_fname { get; set; }
        public string? supplier_mname { get; set; }
        public string? supplier_lname { get; set; }
        public string? supplier_suffix { get; set; }
        public string? supplier_address { get; set; }
        public string? supplier_email {  get; set; }
        public string? supplier_contact_no { get; set; }
        public int? qty { get; set; }
        public string? origin { get; set; }
        public string? purpose { get; set; }
        public DateTime? expected_time_arrival { get; set; }
        public DateTime? expected_time_release { get; set; }
        public DateTime? date_of_inspection { get; set; }
        public DateTime? date_of_registration { get; set; }
        public DateTime? date_of_expiration { get; set; }
        public int? status { get; set; }

        public string? tbl_specification_id { get; set; }
        public string? others { get; set; }

        public bool? is_active { get; set; }
        public int? created_by {  get; set; }
        public int?  modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
        public DateTime? date_due_for_officers { get; set; }
        public bool? coordinatedWithEnforcementDivision { get; set; }

    }
}
