using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class ApplicationModel
    {
        public int? id { get; set; }
        public tbl_application? application { get; set; }
        public string? application_type { get; set; }
        public string? permit_status { get; set; }
        public string? permit_type { get; set; }

        public string? FullName { get; set; }
        public string? Email {  get; set; } 
<<<<<<< HEAD

        public string? comments { get; set; }
=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
    }
}
