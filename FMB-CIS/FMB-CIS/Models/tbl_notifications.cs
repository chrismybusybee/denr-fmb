namespace FMB_CIS.Models
{
    public class tbl_notifications
    {
        public int? id { get; set; }
        public int? source_user_id { get; set; }
        public int? tbl_notification_type_id { get; set; }
        public int? notified_user_id { get; set; }
        public int? notified_user_type { get;set; }
        public string? notification_title { get; set; }
        public string? notification_content { get; set; }
        public DateTime? date_notified { get; set; }
        public bool is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
        public bool? is_about_permit { get; set; }
    }
}
