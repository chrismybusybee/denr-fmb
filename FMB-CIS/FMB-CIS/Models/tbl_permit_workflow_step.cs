namespace FMB_CIS.Models
{
    public class tbl_permit_workflow_step
    {
        public int id { get; set; }
        //public string workflow_step_id { get; set; }
        public int workflow_id { get; set; }
        public string permit_type_code { get; set; }
        public string workflow_code { get; set; }
        public string workflow_step_code { get; set; }
        public string permit_page_code { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public string? on_pre_action { get; set; }
        public string? on_success_action { get; set; }
        public string? on_exit_action { get; set; }
        public bool? is_active { get; set; }
        public int createdBy { get; set; }
        public int modifiedBy { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
