namespace FMB_CIS.Models
{
    public class AnnouncementViewModel
    {
        public int? tbl_announcement_id { get; set; } // id of tbl_announcement
        public string? title { get; set; }
        public int? tbl_announcement_type_id { get; set; } //id of tbl_announcement_type
        public string? tbl_announcement_type_name { get; set; } // name of tbl_announcement_type
        public string? announcement_subject { get; set; } //announcement_subject of tbl_announcement
        public string? announcement_content { get; set; }
        public DateTime? date_publish { get; set; }
        public DateTime? date_expiry { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_published { get; set; }
    }
}
