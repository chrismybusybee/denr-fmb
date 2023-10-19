namespace FMB_CIS.Models
{
    public class tbl_announcement
    {
        public int id { get; set; }
        public string title { get; set; }
        public int tbl_announcement_type_id { get; set; }
        public string announcement_subject { get; set; }
        public string announcement_content { get; set; }
        public DateTime date_publish { get; set; }
        public DateTime date_expiry { get; set; }
        public bool is_active { get; set; }
        public int created_by { get; set; }
        public int modified_by { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_published { get; set; }
    }
}
