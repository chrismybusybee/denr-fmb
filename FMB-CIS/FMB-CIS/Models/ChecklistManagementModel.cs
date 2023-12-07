namespace FMB_CIS.Models
{
    public class ChecklistManagementModel
    {
        public int tbl_document_checklist_id { get; set; }
        public int permit_type_id { get; set; }
        public string tbl_permit_type_name { get; set; }
        public string tbl_document_checklist_name { get; set; }
        public string tbl_document_checklist_description { get; set; }
        public bool is_active { get; set; }
    }
}
