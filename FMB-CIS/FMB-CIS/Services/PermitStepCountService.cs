using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FMB_CIS.Services
{
    public class PermitStepCountService : IPermitStepCount
    {

        private readonly IWorkflowAbstract _workflowService;
        private readonly IWorkflowStepAbstract _workflowStepService;
        private readonly IWorkflowNextStepAbstract _workflowNextStepService;

        public PermitStepCountService
            (IWorkflowAbstract workflowService,
            IWorkflowStepAbstract workflowStepService,
            IWorkflowNextStepAbstract workflowNextStepService)
        {
            _workflowService = workflowService;
            _workflowStepService = workflowStepService;
            _workflowNextStepService = workflowNextStepService;

        }
        public bool CheckWorkflowNextStepIfAccepted(string workflowNextStepId, int regionId)
        {
            var record = _workflowNextStepService.GetRecordByWorkflowNextStepId(workflowNextStepId);
            
            if(record.button_class == "btn-primary")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<PermitProgressStatusModel> MapPermitProgressStatus(int permitTypeId, int regionId)
        {
            string workflowCode = "";
            switch(permitTypeId)
            {
                case 1:
                    workflowCode = "PERMIT_TO_IMPORT_CUSTOM";
                    break;
                case 2:
                    workflowCode = "PERMIT_TO_PURCHASE_CUSTOM";
                    break;
                case 3:
                    workflowCode = "PERMIT_TO_SELL_CUSTOM";
                    break;
                case 5:
                    workflowCode = "AUTHORITY_TO_LEASE_CUSTOM";
                    break;
                case 6:
                    workflowCode = "AUTHORITY_TO_RENT_CUSTOM";
                    break;
                case 7:
                    workflowCode = "AUTHORITY_TO_LEND_CUSTOM";
                    break;
                case 14:
                    workflowCode = "PERMIT_TO_RESELL_CUSTOM";
                    break;
                case 13:
                    workflowCode = "CERTIFICATE_OF_REGISTRATION_CUSTOM";
                    break;

            }
            //var workflow = _workflowService.GetWorkflowByPermitTypeId(permitTypeId);
            var workflow = _workflowService.GetWorkflowByWorkflowCode(workflowCode);
            var workflowId = workflow.workflow_id;
            var worflowStepList = _workflowStepService.GetWorkflowStepsByWorkflowId(workflowId);
            var workflowStepIds = worflowStepList.Select(w => w.workflow_step_id).ToList();
            var workflowNextStepList = _workflowNextStepService.GetWorkflowNextStepListByWorkflowStepIdList(workflowStepIds);

            //List<tbl_permit_workflow_next_step>
            //foreach(var item in  workflowNextStepList)
            //{
            //    var isAccepted = CheckWorkflowNextStepIfAccepted(item.workflow_step_id, regionId);
            //}

            //Get the list of steps
            
            List<PermitProgressStatusModel> progressStatus = new List<PermitProgressStatusModel>();
            //Get the list of nextsteps

            int stepCodeToProceedWithLoop = 1;
            int progress = 0;
            foreach (var currentStep in worflowStepList)
            {
                //currentStep.workflow_step_id;
                var nextSteps = workflowNextStepList
                    .Where(
                    w=>w.workflow_step_id == currentStep.workflow_step_id &&
                    ((w.division_parameter == "notequal" && w.division_id != regionId)
                    || (w.division_parameter == "equal" && w.division_id == regionId)
                    || (w.division_id == 0))
                    ).ToList();
                if(stepCodeToProceedWithLoop == 6)
                {
                    //6 means the user must upload the proof of payment
                    // after uploading the proof of payment, the status will be moved to 7
                    stepCodeToProceedWithLoop = 7; // Change the value to 7
                    progress++;
                    PermitProgressStatusModel progStat = new PermitProgressStatusModel()
                    {
                        //Id = 1,
                        Progress = progress,
                        ApplicationStatusCode = stepCodeToProceedWithLoop.ToString(),
                        isHappyPath = true
                    };
                    progressStatus.Add(progStat);
                }
                if(currentStep.workflow_step_code == stepCodeToProceedWithLoop.ToString())
                {
                    
                    if (currentStep.workflow_step_code == "1")
                    {
                        progress++;
                        PermitProgressStatusModel progStat = new PermitProgressStatusModel()
                        {
                            //Id = 1,
                            Progress = progress,
                            ApplicationStatusCode = "1",
                            isHappyPath = true
                        };
                        progressStatus.Add(progStat);
                    }
                    foreach (var nextStep in nextSteps)
                    {
                        if (nextStep.button_class == "btn-primary")
                        {
                            progress++;
                            PermitProgressStatusModel progStat = new PermitProgressStatusModel()
                            {
                                //Id = 1,
                                Progress = progress,
                                ApplicationStatusCode = nextStep.next_step_code,
                                isHappyPath = true
                            };
                            progressStatus.Add(progStat);
                            stepCodeToProceedWithLoop = Convert.ToInt32(nextStep.next_step_code);
                            //Happy Path
                            //Get Status Code and Proceed with this as next step
                        }
                        else
                        {
                            //Check if the next_step_code is not yet in the list
                            if(progressStatus.Any(ps=>ps.ApplicationStatusCode == nextStep.next_step_code) == false)
                            {
                                PermitProgressStatusModel progStat = new PermitProgressStatusModel()
                                {
                                    //Id = 1,
                                    Progress = progress,
                                    ApplicationStatusCode = nextStep.next_step_code,
                                    isHappyPath = false
                                };
                                progressStatus.Add(progStat);
                                //stepCodeToProceedWithLoop = Convert.ToInt32(nextStep.next_step_code);
                            }
                        }
                    }
                }
                //var nextstepEntity = _context.tbl_permit_workflow_next_step.Where(o => o.workflow_code == model.workflow_code && o.workflow_step_code == workflowStep.workflow_step_code && o.workflow_step_id == workflowStep.workflow_step_id).ToList();
                ////o.workflow_code == model.workflow_code &&
                //workflowStep.nextSteps = nextstepEntity.Adapt<List<WorkflowNextStep>>();
            }

            //var workflowToBeCount = workflowNextStepList
            //    .Where(x =>
            //    x.button_class == "btn-primary" //&&
            //    //((x.division_parameter == "notequal" && x.division_id != regionId)
            //    //|| (x.division_parameter == "equal" && x.division_id == regionId)
            //    //|| (x.division_id == 0)
            //    );
            
            //var workflowByRegionId = workflowToBeCount.Where(x=>x.division_id == regionId);
            //var workflowByRegionIdNotEqual = workflowToBeCount.Where(x=>x.division_id != regionId);
            //int count = progressStatus.Count(p=>p.isHappyPath==true);

            return progressStatus;
        }

        //public int GetTotalProgressCounts (int permitTypeId, int regionId)
        //{
        //    int count = 1;
        //    var workflow = _workflowService.GetWorkflowByPermitTypeId(permitTypeId);
        //    var worflowStepList = _workflowStepService.GetWorkflowStepsByWorkflowId(workflow.workflow_id);
        //    var workflowStepIds = worflowStepList.Select(w => w.workflow_step_id).ToList();
        //    var workflowNextStepList = _workflowNextStepService.GetWorkflowNextStepListByWorkflowStepIdList(workflowStepIds);

        //    int initialApplicationStatus = 1;

        //    return count;
        //}

    }
}
