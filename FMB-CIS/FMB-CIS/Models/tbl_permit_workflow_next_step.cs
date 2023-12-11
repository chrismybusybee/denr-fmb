namespace FMB_CIS.Models
{
    public class tbl_permit_workflow_next_step
    {
        public int id { get; set; }
        public string workflow_next_step_id { get; set; }
        public string workflow_step_id { get; set; }
        public string workflow_id { get; set; }
        public string workflow_code { get; set; }
        public string workflow_step_code { get; set; }
        public string next_step_code { get; set; }
        public int division_id { get; set; }
        public int user_type_id { get; set; }
        public string button_text { get; set; }
        public string button_class { get; set; }
        public bool? is_active { get; set; }
        public int createdBy { get; set; }
        public int modifiedBy { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
