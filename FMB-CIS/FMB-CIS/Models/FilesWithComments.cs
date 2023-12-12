namespace FMB_CIS.Models
{
    public class FilesWithComments
    {
        //from tbl_files
        public int? tbl_files_id { get; set; }
        public string? filename { get; set; }
        public string? tbl_file_sources_id { get; set; }
        public string? tbl_file_type_id { get; set; }
        public int? tbl_user_id { get; set; }
        public int? tbl_application_id { get; set; }
        public string? path { get; set; }
        public int? tbl_files_created_by { get; set; }
        public int? tbl_files_modified_by { get; set; }
        public DateTime? tbl_files_date_created { get; set; }
        public DateTime? tbl_files_date_modified { get; set; }
        public int? file_size { get; set; }
        //public bool? is_active { get; set; }
        //public bool? is_proof_of_payment { get; set; }
        public string? tbl_files_status { get; set; }
        
        //from tbl_comments
        public int? tbl_comments_id { get; set; }
        public string? comment { get; set; }
        public int? tbl_comments_created_by { get; set; }
        public string? tbl_comments_created_by_name { get; set; }
        public int? tbl_comments_modified_by { get; set; }
        public DateTime tbl_comments_date_created { get; set; }
        public DateTime tbl_comments_date_modified { get; set; }
        public string? comment_to { get; set; }
        //from tbl_document_checklist
        public int tbl_document_checklist_id { get; set; }
        public int permit_type_id { get; set; }
        public string tbl_document_checklist_name { get; set; }
        public string tbl_document_checklist_description { get; set; }
    }
}
