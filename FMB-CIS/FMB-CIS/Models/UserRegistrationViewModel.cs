using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserRegistrationViewModel
    {
        //[Required]
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string? suffix { get; set; }
        public string contact_no { get; set; }
        public string valid_id { get; set; }
        public string valid_id_no { get; set; }
        public DateTime birth_date { get; set; }

        public int tbl_region_id { get; set; }
        public int tbl_province_id { get; set; }
        public int tbl_city_id { get; set; }
        public int tbl_brgy_id { get; set; }
        public string street_address { get; set; }
        public string tbl_division_id { get; set; }
        public string email { get; set; }
        [DataType(DataType.Password)]
        //[Required]
        public string password { get; set; }
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
        //public string status { get; set; }
        //public string photo { get; set; }
        public string comment { get; set; }
        [Required]
        public string tbl_user_types_id { get; set; }
        //public DateTime date_created { get; set; }

        //public DateTime date_modified { get; set; }

    }
}
