namespace FMB_CIS.Models
{
    public class tbl_announcement_type
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool is_active { get; set; }
        public int created_by { get; set; }
        public int modified_by { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
    }
}
