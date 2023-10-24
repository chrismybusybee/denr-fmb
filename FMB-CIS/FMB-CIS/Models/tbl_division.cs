namespace FMB_CIS.Models
{
    public class tbl_division
    {
        public int? id { get; set; }
        public string? office_name { get; set; }
        public string? department { get; set; }
        public int? region_id { get; set; }
        public int? province_id { get; set; }
        public string? company_name { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
