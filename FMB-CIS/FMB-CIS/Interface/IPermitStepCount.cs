using FMB_CIS.Models;

namespace FMB_CIS.Interface
{
    public interface IPermitStepCount
    {
        List<PermitProgressStatusModel> MapPermitProgressStatus(int permitTypeId, int regionId);
        bool CheckWorkflowNextStepIfAccepted(string workflowNextStepId, int regionId);
    }
}
