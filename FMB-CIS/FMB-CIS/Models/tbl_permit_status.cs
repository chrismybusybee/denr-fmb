using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_permit_status
    {
        public int id { get; set; }

        public string status { get; set; }
        public int application_type { get; set; }
    }
}