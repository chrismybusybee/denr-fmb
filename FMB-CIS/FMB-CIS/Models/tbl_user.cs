using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_user
    {
        public int? id { get; set; }

        public string? FullName { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set;}
        public string? middle_name { get; set; }
        public string? suffix { get; set; }

        public int? tbl_user_types_id { get; set; }

        public string? contact_no { get; set; }
        public string?   valid_id { get; set; }
        public string? valid_id_no {  get; set; }
        public string? birth_date { get; set; }

        public string? email {  get; set; }
    }
}
