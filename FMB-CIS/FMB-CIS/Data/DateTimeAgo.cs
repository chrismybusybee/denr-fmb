namespace FMB_CIS.Data
{
    public class DateTimeAgo
    {
        public static string TimeAgo(DateTime dateTime)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "1s ago" : ts.Seconds + "s ago";

            if (delta < 2 * MINUTE)
                return "1m ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + "m ago";

            if (delta < 90 * MINUTE)
                return "1h ago";

            if (delta < 24 * HOUR)
                return ts.Hours + "h ago";

            if (delta < 48 * HOUR)
                return "1d ago";

            if (delta < 30 * DAY)
                return ts.Days + "d ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "1m ago" : months + "m ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "1y ago" : years + "y ago";
            }
        }
    }
}
