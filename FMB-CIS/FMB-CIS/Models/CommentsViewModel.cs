namespace FMB_CIS.Models
{
    public class CommentsViewModel
    {
        //public int id { get; set; }
        public int tbl_user_id { get; set; }
        public int tbl_application_id { get; set; }
        public int tbl_files_id { get; set; }
        public string fileName { get; set; }
        public string comment { get; set; }
        public int created_by { get; set; }
        public int modified_by { get; set; }
        public string commenterName { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
    }
}
