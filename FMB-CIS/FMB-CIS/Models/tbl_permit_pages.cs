namespace FMB_CIS.Models
{
    public class tbl_permit_pages
    {
        public int id { get; set; }
        public string? permit_page_code { get; set; }
        public string? permit_type_code { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
