using System.ComponentModel.DataAnnotations.Schema;

namespace FMB_CIS.Models
{
    public class tbl_user_activitylog
    {
        public int id { get; set; }
        public int UserId { get; set; }
        public string Entity { get; set; }
        public string UserAction { get; set; }
        public string Remarks { get; set; }
        public DateTime? CreatedDt { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime? ApkDatetime { get; set; }
        public string Source { get; set; }
    }
}
