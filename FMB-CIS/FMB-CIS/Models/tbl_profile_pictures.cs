namespace FMB_CIS.Models
{
    public class tbl_profile_pictures
    {
        public int? id { get; set; }
        public string? file_type { get; set; }
        public int? tbl_user_id { get; set; }
        public string? filename { get; set; }
        public string? path { get; set; }
        public string? webPath { get; set; }
        public int? file_size { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
