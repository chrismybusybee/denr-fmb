namespace FMB_CIS.Models
{
    public class tbl_application_read
    {
        public int id { get; set; }
        public int tbl_application_id { get; set; }
        public int tbl_user_id { get; set; }
        public bool is_read { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
    }
}
