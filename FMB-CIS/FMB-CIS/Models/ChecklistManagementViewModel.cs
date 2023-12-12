namespace FMB_CIS.Models
{
    public class ChecklistManagementViewModel
    {

        public IEnumerable<tbl_document_checklist>? tbl_Document_Checklists { get; set; }
        public IEnumerable<tbl_permit_type>? tbl_Permit_Types { get; set; }

        public tbl_document_checklist? tbl_document_Checklist { get; set; }
        public tbl_permit_type? tbl_permit_Type { get; set; }
        public IEnumerable<ChecklistManagementModel>? checklistManagementModels { get; set; }
        public ChecklistManagementModel? checklistManagementModel { get; set; }

    }
}
