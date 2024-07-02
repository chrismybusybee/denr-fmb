using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FMB_CIS.Services
{
    public class WorkflowNextStepService : IAbstractEntry<tbl_permit_workflow_next_step>
    {
        private readonly LocalContext _context;
        public WorkflowNextStepService(LocalContext context)
        {
            _context = context;
        }
        public async Task<List<tbl_permit_workflow_next_step>> GetRecords()
        {
            var record = await _context.tbl_permit_workflow_next_step.AsNoTracking().ToListAsync();

            return record;
        }

        public async Task<tbl_permit_workflow_next_step> GetRecordById(int id)
        {
            var record = await _context.tbl_permit_workflow_next_step.AsNoTracking().Where(o => o.id == id).FirstOrDefaultAsync();

            return record;
        }
        public async Task InsertRecord(tbl_permit_workflow_next_step model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.createdBy = userId;
            _context.tbl_permit_workflow_next_step.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecord(tbl_permit_workflow_next_step selectedModel, int userId)
        {
            var model = _context.tbl_permit_workflow_next_step.SingleOrDefault(x => x.id == selectedModel.id);
            if (model != null)
            {
                model.workflow_next_step_id = selectedModel.workflow_next_step_id;
                model.workflow_step_id = selectedModel.workflow_step_id;
                model.workflow_id = selectedModel.workflow_id;
                model.workflow_code = selectedModel.workflow_code;
                model.workflow_step_code = selectedModel.workflow_step_code;
                model.next_step_code = selectedModel.next_step_code;
                model.division_id = selectedModel.division_id;
                model.division_parameter = selectedModel.division_parameter;
                model.button_text = selectedModel.button_text;
                model.button_class = selectedModel.button_class;
                model.is_active = selectedModel.is_active;
                model.modifiedBy = selectedModel.modifiedBy;
                model.date_modified = selectedModel.date_modified;

                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteRecord(tbl_permit_workflow_next_step model)
        {
            _context.tbl_permit_workflow_next_step.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecords(List<tbl_permit_workflow_next_step> modelList)
        {
            var selectedConsultantTypes = _context.tbl_permit_workflow_next_step.Where(x => modelList.Contains(x)).ExecuteDelete();
            await _context.SaveChangesAsync();
        }
    }
}
