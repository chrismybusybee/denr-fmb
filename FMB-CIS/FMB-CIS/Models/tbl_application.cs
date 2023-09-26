using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_application
    {
        public int id { get; set; }

        public int tbl_user_id { get; set; }
        public int tbl_application_type_id { get; set; }
        public int tbl_permit_type_id { get; set; }
        public int status { get; set; }
        public int modified_by { get; set; }
        public DateTime date_modified { get; set; }
        //public string? Status { get; set; }
        //public ICollection<tbl_application_type> application_type { get; set; }

}
}
