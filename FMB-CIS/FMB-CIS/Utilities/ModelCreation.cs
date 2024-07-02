using FMB_CIS.Models;

namespace FMB_CIS.Utilities
{
    public static class ModelCreation
    {
        public static tbl_notifications PermitNotificationForApproverModel(string notifTitle, string notifContent, int approverUserTypeId, int loggedInUserId, int loggedInUserRegionId)
        {
            tbl_notifications model = new tbl_notifications()
            {
                source_user_id = null,
                tbl_notification_type_id = 2,
                notified_user_id = null,
                notified_user_type = approverUserTypeId,
                notification_title = notifTitle,
                notification_content = notifContent,
                date_notified = DateTime.Now,
                is_active = true,
                created_by = loggedInUserId,
                modified_by = loggedInUserId,
                date_created = DateTime.Now,
                date_modified = DateTime.Now,
                is_about_permit = false,
                is_region_exclusive = true,
                region_id_filter = loggedInUserRegionId
            };

            return model;
        }
        public static tbl_notifications PermitNotificationForApplicantModel(string notifTitle, string notifContent, int notifiedUserId, int loggedInUserId)
        {
            tbl_notifications model = new tbl_notifications()
            {
                source_user_id = null,
                tbl_notification_type_id = 1,
                notified_user_id = notifiedUserId,
                notified_user_type = null,
                notification_title = notifTitle,
                notification_content = notifContent,
                date_notified = DateTime.Now,
                is_active = true,
                created_by = loggedInUserId,
                modified_by = loggedInUserId,
                date_created = DateTime.Now,
                date_modified = DateTime.Now,
                is_about_permit = false, 
            };

            return model;
        }
    }
}
