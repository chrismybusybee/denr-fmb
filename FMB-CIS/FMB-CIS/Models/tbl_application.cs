using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace FMB_CIS.Models
{
    public class tbl_application
    {
<<<<<<< HEAD
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
        public string? expected_time_arrival { get; set; }
        public string? expected_time_release { get; set; }
        public string? date_of_inspection { get; set; }
        public int? status { get; set; }

        public string? others { get; set; }

        public bool? is_active { get; set; }
        public int? created_by {  get; set; }
        public int?  modified_by { get; set; }
        
=======
        public int id { get; set; }
        public int tbl_user_id { get; set; }
        public int tbl_application_type_id { get; set; }
        public int tbl_permit_type_id { get; set; }
        public string supplier_fname { get; set; }
        public string supplier_mname { get; set; }
        public string supplier_lname { get; set; }
        public string supplier_suffix { get; set; }
        public string supplier_address { get; set; }
        public string supplier_email {  get; set; }
        public string supplier_contact_no { get; set; }
        public string qty { get; set; }
        public string origin { get; set; }
        public string purpose { get; set; }
        public string expected_time_arrival { get; set; }
        public string expected_time_release { get; set; }
        public string date_of_inspection { get; set; }
        public int status { get; set; }
<<<<<<< HEAD
>>>>>>> Added model, controller for chainsaw seller
=======

        public int is_active { get; set; }
        public int created_by {  get; set; }
        public int modified_by { get; set; }
        
>>>>>>> Created a chainsaw owner controller
    }
}
