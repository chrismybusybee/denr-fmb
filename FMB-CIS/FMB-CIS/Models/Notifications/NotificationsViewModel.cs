namespace FMB_CIS.Models.Notifications
{
    public class userNotificationsWithRead : tbl_notifications
    {
        public bool? is_read { get; set; }
    }
    public class NotificationsViewModel
    {
        public List<tbl_notifications>? tbl_Notifications { get; set; }
        public tbl_notifications? tbl_Notification { get; set; }
        public List<userNotificationsWithRead>? userNotifications { get; set; }
        public userNotificationsWithRead? userNotification { get; set; }
    }

    


}
