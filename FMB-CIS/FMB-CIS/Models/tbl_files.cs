namespace FMB_CIS.Models
{
    public class tbl_files
    {
        public int Id { get; set; }
        public string filename { get; set; }
        public string tbl_file_sources_id { get; set; }
        public string tbl_file_type_id { get; set;}
        public int? tbl_application_id { get; set; }
        public string path { get; set; }
        public int created_by {  get; set; }
        public int modified_by { get; set;}
        public DateTime date_created { get; set; } 
        public DateTime date_modified { get; set;}
    }
}
