namespace FMB_CIS.Models
{
    public class tbl_files_checklist_bridge
    {
        public int id {  get; set; }
        public int? tbl_files_id { get; set; }
        public int? tbl_document_checklist_id { get; set; }
        public string? status { get; set; }
    }
}
