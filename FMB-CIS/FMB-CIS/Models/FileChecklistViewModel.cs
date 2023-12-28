namespace FMB_CIS.Models
{
    public class FileChecklistViewModel
    {
        public string? FileName { get; set; }
        public int? tbl_document_checklist_id { get; set; }
        public string? tbl_document_checklist_name { get; set; }

        //used for selectize
        public List<string>? FileNames { get; set; }
    }
}
