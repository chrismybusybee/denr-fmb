using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Mvc;

namespace FMB_CIS.Utilities
{
    public class PermitUtilities
    {
        private readonly IWorkflowAbstract _workflowService;
        private readonly IWorkflowStepAbstract _workflowStepService;
        private readonly IWorkflowNextStepAbstract _workflowNextStepService;

        public PermitUtilities
            (IWorkflowAbstract workflowService,
            IWorkflowStepAbstract workflowStepService, 
            IWorkflowNextStepAbstract workflowNextStepService)
        {
           _workflowService = workflowService;
           _workflowStepService = workflowStepService;
           _workflowNextStepService = workflowNextStepService;
           
        }
        public int GetTotalStepsCount(int permitTypeId, int regionId)
        {
            var workflow = _workflowService.GetWorkflowByPermitTypeId(permitTypeId);
            var workflowId = workflow.workflow_id;
            var worflowStepList = _workflowStepService.GetWorkflowStepsByWorkflowId(workflowId);
            var workflowStepIds = worflowStepList.Select(w=> w.workflow_id).ToList();
            var workflowNextStepList = _workflowNextStepService.GetWorkflowNextStepListByWorkflowStepIdList(workflowStepIds);

            int count = workflowNextStepList
                .Where(x => 
                x.button_class == "btn-primary" && 
                ((x.division_parameter == "notequal" && x.division_id != regionId)
                ||(x.division_parameter =="equal" && x.division_id == regionId)
                ||(x.division_parameter == "0")
                ))
                .Count();

            return count;
        }
    }
}
