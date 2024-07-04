using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FMB_CIS.Services
{
    public class ApplicationService : IApplicationAbstract
    {
        private readonly LocalContext _context;
        public ApplicationService(LocalContext context)
        {
            _context = context;
        }
        public async Task<List<tbl_application>> GetRecords()
        {
            var record = await _context.tbl_application.AsNoTracking().ToListAsync();

            return record;
        }

        public async Task<tbl_application> GetRecordById(int id)
        {
            var record = await _context.tbl_application.AsNoTracking().Where(o => o.id == id).FirstOrDefaultAsync();

            return record;
        }
        public async Task InsertRecord(tbl_application model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.created_by = userId;
            _context.tbl_application.Add(model);
            await _context.SaveChangesAsync();
        }

        
        public async Task UpdateRecord(tbl_application selectedModel, int userId)
        {
            var model = _context.tbl_application.SingleOrDefault(x => x.id == selectedModel.id);
            if (model != null)
            {
                model.id = selectedModel.id;
                model.tbl_user_id = selectedModel.tbl_user_id;
                model.tbl_application_type_id = selectedModel.tbl_application_type_id;
                model.tbl_permit_type_id = selectedModel.tbl_permit_type_id;
                model.supplier_fname = selectedModel.supplier_fname;
                model.supplier_mname = selectedModel.supplier_mname;
                model.supplier_lname = selectedModel.supplier_lname;
                model.supplier_suffix = selectedModel.supplier_suffix;
                model.supplier_contact_no = selectedModel.supplier_contact_no;
                model.supplier_address = selectedModel.supplier_address;
                model.supplier_email = selectedModel.supplier_email;
                model.origin = selectedModel.origin;
                model.qty = selectedModel.qty;
                //model.new_owner = selectedModel.new_owner;
                //model.OR = selectedModel.OR;
                //model.seller = selectedModel.seller;
                //model.acquire = selectedModel.acquire;
                //model.renewal = selectedModel.renewal;
                //model.brand = selectedModel.brand;
                //model.serial = selectedModel.serial;
                //model.tbl_specification_id = selectedModel.tbl_specification_id;
                //model.total_price = selectedModel.total_price;
                //model.lend = selectedModel.lend;
                model.purpose = selectedModel.purpose;
                model.others = selectedModel.others;
                model.expected_time_arrival = selectedModel.expected_time_arrival;
                model.expected_time_release = selectedModel.expected_time_release;
                model.date_of_inspection = selectedModel.date_of_inspection;
                model.date_of_registration = selectedModel.date_of_registration;
                model.date_of_expiration = selectedModel.date_of_expiration;
                model.is_active = selectedModel.is_active;
                model.created_by = selectedModel.created_by;
                model.modified_by = selectedModel.modified_by;
                model.date_created = selectedModel.date_created;
                model.date_modified = selectedModel.date_modified;
                model.date_due_for_officers = selectedModel.date_due_for_officers;
                model.status = selectedModel.status;
                model.coordinatedWithEnforcementDivision = selectedModel.coordinatedWithEnforcementDivision;
                model.initial_date_of_inspection = selectedModel.initial_date_of_inspection;
                model.renew_from = selectedModel.renew_from;
                model.new_email_address = selectedModel.new_email_address;
                model.ReferenceNo = selectedModel.ReferenceNo;
                model.original_renew_from = selectedModel.renew_from;
                //model.progress_count = selectedModel.progress_count;

                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteRecord(tbl_application model)
        {
            _context.tbl_application.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecords(List<tbl_application> modelList)
        {
            var selectedConsultantTypes = _context.tbl_application.Where(x => modelList.Contains(x)).ExecuteDelete();
            await _context.SaveChangesAsync();
        }

    }
}
