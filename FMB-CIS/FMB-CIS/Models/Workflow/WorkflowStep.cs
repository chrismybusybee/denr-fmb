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

enum WorkflowStepEnum : ushort
{
    PERMIT_TO_IMPORT_APPLICATION_SUBMISSION = 0,
    PERMIT_TO_IMPORT_APPLICATION_ACCEPTANCE = 1,
    PERMIT_TO_IMPORT_PHYSICAL_INSPECTION = 2,
    PERMIT_TO_IMPORT_PAYMENT_OF_FEES = 3,
    PERMIT_TO_IMPORT_PAYMENT_EVALUATION = 4,
    PERMIT_TO_IMPORT_PERMIT_APPROVAL = 5,
    PERMIT_TO_IMPORT_PERMIT_ISSUANCE = 6
}

enum PermitStatusEnum : ushort
{
    APPLICATION_SUBMISSION = 1,
    APPLICATION_ACCEPTANCE_REJECT = 2,
    FOR_PHYSICAL_INSPECTION = 3,
    PHYSICAL_INSPECTION_APPROVED = 4,
    PHYSICAL_INSPECTION_REJECT = 5,
    PAYMENT_OF_FEES = 6,
    PAYMENT_EVALUATION = 7,
    PAYMENT_EVALUATION_REJECT = 8,
    PERMIT_APPROVAL = 9,
    PERMIT_APPROVAL_REJECT = 10,
    PERMIT_ISSUANCE = 11,
    PENRO_FOCAL_APPROVAL = 12,
    PENRO_FOCAL_APPROVAL_REJECT = 13,
    PENRO_APPROVAL = 14,
    PENRO_APPROVAL_REJECT = 15,
    REGION_FOCAL_APPROVAL = 16,
    REGION_FOCAL_APPROVAL_REJECT = 17,
    REGION_APPROVAL = 18,
    REGION_APPROVAL_REJECT = 19
}

enum PermitTypesEnum : ushort
{
    PERMIT_TO_IMPORT = 1,
    PERMIT_TO_PURCHASE = 2,
    PERMIT_TO_SELL = 3,
    PERMIT_TO_TRANSFER_OWNERSHIP = 4,
    AUTHORITY_TO_LEASE = 5,
    AUTHORITY_TO_RENT = 6,
    AUTHORITY_TO_LEND = 7,
    CERTIFICATE_OF_REGISTRATION = 13,
    PERMIT_TO_RESELL = 14
}