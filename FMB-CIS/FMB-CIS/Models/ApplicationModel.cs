using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class ApplicationModel
    {
        public int? id { get; set; }
        public tbl_application application { get; set; }
        public string application_type { get; set; }
        public string permit_status { get; set; }
        public string permit_type { get; set; }
        public int status { get; set; }
        public string? FullName { get; set; }
        public string? Email {  get; set; } 
        public string? comments { get; set; }

    }
}
enum ApplicationTypeEnum : ushort
{
    CHAINSAW_OWNER = 1,
    CHAINSAW_IMPORTER = 2,
    CHAINSAW_SELLER = 3
}
