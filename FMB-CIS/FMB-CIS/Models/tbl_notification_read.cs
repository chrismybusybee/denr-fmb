namespace FMB_CIS.Models
{
    public class tbl_notification_read
    {
        public int? id { get; set; }
        public int? tbl_user_id { get; set; }
        public int? tbl_notifications_id { get; set; }
        public bool? is_read { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
