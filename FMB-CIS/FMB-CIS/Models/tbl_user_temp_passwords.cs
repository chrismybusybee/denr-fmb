namespace FMB_CIS.Models
{
    public class tbl_user_temp_passwords
    {
        public int id { get; set; }
        public int? tbl_user_id { get; set; }
        public string? password { get; set; }
        public DateTime? password_expiry { get; set; }
        public bool? is_active { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
