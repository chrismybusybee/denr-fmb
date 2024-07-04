using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FMB_CIS.Services
{
    public class WorkflowStepService : IWorkflowStepAbstract
    {
        private readonly LocalContext _context;
        public WorkflowStepService(LocalContext context)
        {
            _context = context;
        }
        public async Task<List<tbl_permit_workflow_step>> GetRecords()
        {
            var record = await _context.tbl_permit_workflow_step.AsNoTracking().ToListAsync();

            return record;
        }

        public async Task<tbl_permit_workflow_step> GetRecordById(int id)
        {
            var record = await _context.tbl_permit_workflow_step.AsNoTracking().Where(o => o.id == id).FirstOrDefaultAsync();

            return record;
        }
        public Task<tbl_user> GetNextStepApprover(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task InsertRecord(tbl_permit_workflow_step model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.createdBy = userId;
            _context.tbl_permit_workflow_step.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecord(tbl_permit_workflow_step selectedModel, int userId)
        {
            var model = _context.tbl_permit_workflow_step.SingleOrDefault(x => x.id == selectedModel.id);
            if (model != null)
            {
                model.workflow_step_id = selectedModel.workflow_step_id;
                model.permit_type_code = selectedModel.permit_type_code;
                model.workflow_code = selectedModel.workflow_code;
                model.workflow_step_code = selectedModel.workflow_step_code;
                model.name = selectedModel.name;
                model.description = selectedModel.description;
                model.on_pre_action = selectedModel.on_pre_action;
                model.on_success_action = selectedModel.on_success_action;
                model.on_exit_action = selectedModel.on_exit_action;

                model.is_active = selectedModel.is_active;
                model.modifiedBy = selectedModel.modifiedBy;
                model.date_modified = selectedModel.date_modified;
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteRecord(tbl_permit_workflow_step model)
        {
            _context.tbl_permit_workflow_step.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecords(List<tbl_permit_workflow_step> modelList)
        {
            var selectedConsultantTypes = _context.tbl_permit_workflow_step.Where(x => modelList.Contains(x)).ExecuteDelete();
            await _context.SaveChangesAsync();
        }

        public List<tbl_permit_workflow_step> GetWorkflowStepsByWorkflowId(string workflowId)
        {
            var records = _context.tbl_permit_workflow_step.AsNoTracking().Where(o => o.workflow_id == workflowId.ToString()).ToList();
            return records;
        }
    }
}
