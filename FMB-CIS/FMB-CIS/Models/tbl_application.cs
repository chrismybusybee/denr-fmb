using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_application
    {
        public int id { get; set; }

        public int tbl_user_id { get; set; }
        public int tbl_application_type_id { get; set; }
        public int tbl_permit_type_id { get; set; }
        //public string? Status { get; set; }
        //public ICollection<tbl_application_type> application_type { get; set; }

}
}
