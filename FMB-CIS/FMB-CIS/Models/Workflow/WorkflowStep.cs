using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class WorkflowStep
    {
        public int id { get; set; }
        public string workflow_id { get; set; }
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
        public IEnumerable<WorkflowNextStep> nextSteps { get; set; }
    }
}

enum WorkFlowStepEnum : ushort
{
    PERMIT_TO_IMPORT_APPLICATION_SUBMISSION = 0,
    PERMIT_TO_IMPORT_APPLICATION_ACCEPTANCE = 1,
    PERMIT_TO_IMPORT_PHYSICAL_INSPECTION = 2,
    PERMIT_TO_IMPORT_PAYMENT_OF_FEES = 3,
    PERMIT_TO_IMPORT_PAYMENT_EVALUATION = 4,
    PERMIT_TO_IMPORT_PERMIT_APPROVAL = 5,
    PERMIT_TO_IMPORT_PERMIT_ISSUANCE = 6
}