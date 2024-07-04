using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FMB_CIS.Services
{
    public class WorkflowService : IWorkflowAbstract
    {
        private readonly LocalContext _context;
        public WorkflowService(LocalContext context)
        {
            _context = context;
        }
        public async Task<List<tbl_permit_workflow>> GetRecords()
        {
            var record = await _context.tbl_permit_workflow.AsNoTracking().ToListAsync();

            return record;
        }

        public async Task<tbl_permit_workflow> GetRecordById(int id)
        {
            var record = await _context.tbl_permit_workflow.AsNoTracking().Where(o => o.id == id).FirstOrDefaultAsync();

            return record;
        }
        public async Task<List<int>> GetNextStepApprover(int permitTypeId, int currentStatus)
        {
            //var user = await _context.tbl_user.FindAsync(userId);

            //var workflowNextStep = await _context.tbl_permit_workflow_next_step.FirstOrDefaultAsync(x=>x.user_type_id == user.tbl_user_types_id);

            //var nextApproverUserType = workflowNextStep.user_type_id;

            //return nextApproverUserType;

            Workflow workflowModel = new Workflow();
            //Get the list of users
            string permitTypeQueryParameter = "";

            switch (permitTypeId)
            {
                case 1:
                    permitTypeQueryParameter = "PERMIT_TO_IMPORT_CUSTOM";
                    break;
                case 2:
                    permitTypeQueryParameter = "PERMIT_TO_PURCHASE_CUSTOM";
                    break;
                case 3:
                    permitTypeQueryParameter = "PERMIT_TO_SELL_CUSTOM";
                    break;
                case 5:
                    permitTypeQueryParameter = "AUTHORITY_TO_LEASE_CUSTOM";
                    break;
                case 6:
                    permitTypeQueryParameter = "AUTHORITY_TO_RENT_CUSTOM";
                    break;
                case 7:
                    permitTypeQueryParameter = "AUTHORITY_TO_LEND_CUSTOM";
                    break;
                case 14:
                    permitTypeQueryParameter = "PERMIT_TO_RESELL_CUSTOM";
                    break;
                case 13:
                    permitTypeQueryParameter = "CERTIFICATE_OF_REGISTRATION_CUSTOM";
                    break;
            }
            var workflow = _context.tbl_permit_workflow.FirstOrDefault(e => e.workflow_code == permitTypeQueryParameter);
            workflowModel = workflow.Adapt<Workflow>();

            //Get the list of steps
            var stepEntity = _context.tbl_permit_workflow_step.Where(o => o.workflow_code == workflowModel.workflow_code).ToList();
            workflowModel.steps = stepEntity.Adapt<List<WorkflowStep>>();

            //Get the list of nextsteps
            foreach (WorkflowStep workflowStep in workflowModel.steps)
            {
                var nextstepEntity = _context.tbl_permit_workflow_next_step.Where(o => o.workflow_code == workflowModel.workflow_code && o.workflow_step_code == workflowStep.workflow_step_code).ToList();
                //o.workflow_code == model.workflow_code &&
                workflowStep.nextSteps = nextstepEntity.Adapt<List<WorkflowNextStep>>();
            }

            List<int> userTypeIds = new List<int>();

            foreach (WorkflowStep workflowStep in workflowModel.steps)
            {
                if (workflowStep.workflow_step_code == currentStatus.ToString())
                {
                    foreach (WorkflowNextStep workflowNextStep in workflowStep.nextSteps)
                    {
                        userTypeIds.Add(workflowNextStep.user_type_id);

                        //if (workflowNextStep.user_type_id.ToString() == ((ClaimsIdentity)User.Identity).FindFirst("userRoleIds").Value
                        //&& (workflowNextStep.division_id == 0 ||
                        //(workflowNextStep.division_parameter == "equal" && workflowNextStep.division_id == Model.applicantViewModels.tbl_region_id) ||
                        //(workflowNextStep.division_parameter == "notequal" && workflowNextStep.division_id != Model.applicantViewModels.tbl_region_id)))
                        //{

                        //    workflowNextStep.button_text

                        //}
                    }
                }
            }

            var distinctUserTypeIds = userTypeIds.Distinct().ToList();

            return distinctUserTypeIds;

        }

        public tbl_permit_workflow GetWorkflowByPermitTypeId(int permitTypeId)
        {
            //if (permitTypeId != 0)
            //{
                var record = _context.tbl_permit_workflow.AsNoTracking().Where(o => o.permit_type_code == permitTypeId.ToString()).FirstOrDefault();
                return record;
            //}
        }
        public tbl_permit_workflow GetWorkflowByWorkflowCode(string workflowCode)
        {
            //if (permitTypeId != 0)
            //{
                var record = _context.tbl_permit_workflow.AsNoTracking().Where(o => o.workflow_code == workflowCode).FirstOrDefault();
                return record;
            //}
        }

        public async Task<List<tbl_permit_workflow>> GetWorkflowListByPermitTypeId(int permitTypeId)
        {
                var records = await _context.tbl_permit_workflow.AsNoTracking().Where(o => o.permit_type_code == permitTypeId.ToString()).ToListAsync();
                return records;
        }
        public async Task InsertRecord(tbl_permit_workflow model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.createdBy = userId;
            _context.tbl_permit_workflow.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecord(tbl_permit_workflow selectedModel, int userId)
        {
            var model = _context.tbl_permit_workflow.SingleOrDefault(x => x.id == selectedModel.id);

            if (model != null)
            {
                model.workflow_id = selectedModel.workflow_id;
                model.permit_type_code = selectedModel.permit_type_code;
                model.workflow_code = selectedModel.workflow_code;
                model.name = selectedModel.name;
                model.description = selectedModel.description;
                model.is_active = selectedModel.is_active;
                model.date_modified = selectedModel.date_modified;
                model.modifiedBy = selectedModel.modifiedBy;
                await _context.SaveChangesAsync();
            }

        }
        public async Task DeleteRecord(tbl_permit_workflow model)
        {
            _context.tbl_permit_workflow.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecords(List<tbl_permit_workflow> modelList)
        {
            var selectedConsultantTypes = _context.tbl_permit_workflow.Where(x => modelList.Contains(x)).ExecuteDelete();
            await _context.SaveChangesAsync();
        }

    }
}
