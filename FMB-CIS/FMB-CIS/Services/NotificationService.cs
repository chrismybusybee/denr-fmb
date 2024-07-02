using FMB_CIS.Data;
using FMB_CIS.Interface;
using FMB_CIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FMB_CIS.Services
{
    public class NotificationService : INotificationAbstract
    {
        private readonly LocalContext _context;
        public NotificationService(LocalContext context)
        {
            _context = context;
        }
        public async Task<List<tbl_notifications>> GetRecords()
        {
            var record = await _context.tbl_notifications.AsNoTracking().ToListAsync();

            return record;
        }

        public async Task<tbl_notifications> GetRecordById(int id)
        {
            var record = await _context.tbl_notifications.AsNoTracking().Where(o => o.id == id).FirstOrDefaultAsync();

            return record;

        }
        public void Insert(tbl_notifications model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.created_by = userId;
            _context.tbl_notifications.Add(model);
            _context.SaveChanges();
        }
        public async Task InsertRecord(tbl_notifications model, int userId)
        {
            model.date_created = DateTime.UtcNow;
            model.created_by = userId;
            _context.tbl_notifications.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecord(tbl_notifications selectedModel, int userId)
        {
            var model = _context.tbl_notifications.SingleOrDefault(x => x.id == selectedModel.id);

            if (model != null)
            {
                model.source_user_id = selectedModel.source_user_id;
                model.tbl_notification_type_id = selectedModel.source_user_id;
                model.notified_user_id = selectedModel.source_user_id;
                model.notified_user_type = selectedModel.notified_user_type;
                model.notification_title = selectedModel.notification_title;
                model.notification_content = selectedModel.notification_content;
                model.date_notified = selectedModel.date_notified;
                model.is_active = selectedModel.is_active;
                model.created_by = selectedModel.created_by;
                model.date_created = selectedModel.date_created;
                model.is_about_permit = selectedModel.is_about_permit;

                model.modified_by = userId;
                model.date_modified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

        }
        public async Task DeleteRecord(tbl_notifications model)
        {
            _context.tbl_notifications.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecords(List<tbl_notifications> modelList)
        {
            var selectedConsultantTypes = _context.tbl_notifications.Where(x => modelList.Contains(x)).ExecuteDelete();
            await _context.SaveChangesAsync();
        }


    }
}
