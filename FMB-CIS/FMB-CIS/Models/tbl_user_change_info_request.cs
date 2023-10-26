using System.Web.Helpers;

namespace FMB_CIS.Models
{
    public class tbl_user_change_info_request
    {
        public int? id { get; set; }
        public string? ticket_no { get; set; }
        public int? tbl_user_id { get; set; }
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public string? last_name { get; set; }
        public string? suffix { get; set; }
        public string? company_name { get; set; }
        public string? contact_no { get; set; }
        public string? valid_id { get; set; }
        public string? valid_id_no { get; set; }
        public DateTime? birth_date { get; set; }
        public int? tbl_region_id { get; set; }
        public int? tbl_province_id { get; set; }
        public int? tbl_city_id { get; set; }
        public int? tbl_brgy_id { get; set; }
        public string? street_address { get; set; }
        public string? email { get; set; }
        public int? tbl_user_types_id { get; set; }
        public string? user_classification { get; set; }
        public string? gender { get; set; }
        public int? tbl_user_change_info_request_status_id { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
