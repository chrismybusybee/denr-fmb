using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class ApplicationModel
    {
        public int id { get; set; }
        public tbl_application application { get; set; }
        public string application_type { get; set; }
        public string permit_status { get; set; }
        public string permit_type { get; set; }
        public int status { get; set; }
    }
}
