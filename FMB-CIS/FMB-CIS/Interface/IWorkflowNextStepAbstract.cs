using FMB_CIS.Models;

namespace FMB_CIS.Interface
{
    public interface IWorkflowNextStepAbstract : IAbstractEntry<tbl_permit_workflow_next_step>
    {
        Task<List<tbl_permit_workflow_next_step>> GetWorkflowNextStepsByNextStepCode(int nextStepCode);
        List<tbl_permit_workflow_next_step> GetWorkflowNextStepListByWorkflowStepIdList(List<string> workflowStepIds);
        tbl_permit_workflow_next_step GetRecordByWorkflowNextStepId(string id);
    }
}
