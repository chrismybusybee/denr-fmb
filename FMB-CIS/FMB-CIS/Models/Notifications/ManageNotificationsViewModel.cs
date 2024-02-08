namespace FMB_CIS.Models.Notifications
{
    public class ManageNotificationsViewModel
    {
        public List<tbl_notifications>? tbl_Notifications { get; set; }
        public tbl_notifications? tbl_Notification { get; set; }
        public List<tbl_notification_type>? tbl_Notification_Types { get; set; }
        public List<NotificationsJoinedValues>? notificationsJoinedValues { get; set; }
        public NotificationsJoinedValues? notificationsJoinedValue { get; set; }
        public List<tbl_user_types>? tbl_User_Types { get; set; }
        public List<tbl_user>? tbl_Users { get; set; }
    }
}
