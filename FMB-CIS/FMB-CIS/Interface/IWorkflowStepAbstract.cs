using FMB_CIS.Models;

namespace FMB_CIS.Interface
{
    public interface IWorkflowStepAbstract : IAbstractEntry<tbl_permit_workflow_step>
    {
        List<tbl_permit_workflow_step> GetWorkflowStepsByWorkflowId(string workflowId);
    }
}
