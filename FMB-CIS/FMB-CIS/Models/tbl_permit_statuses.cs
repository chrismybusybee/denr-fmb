using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_permit_statuses
    {
        public int id { get; set; }
        public string status { get; set; }
        public string comment { get; set; }
        public int application_type { get; set; }
    }
}