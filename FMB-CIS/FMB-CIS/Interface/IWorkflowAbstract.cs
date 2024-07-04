using FMB_CIS.Models;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace FMB_CIS.Interface
{
    public interface IWorkflowAbstract : IAbstractEntry<tbl_permit_workflow>
    {
        Task<List<int>> GetNextStepApprover(int permitTypeId, int currentStatus);
        tbl_permit_workflow GetWorkflowByPermitTypeId(int permitTypeId);
        tbl_permit_workflow GetWorkflowByWorkflowCode(string workflowCode);
        Task<List<tbl_permit_workflow>> GetWorkflowListByPermitTypeId(int permitTypeId);
    }
}
