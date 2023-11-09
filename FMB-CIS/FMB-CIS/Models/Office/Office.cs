using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class Office
    {
        public int id { get; set; }
        public string? office_name { get; set; }
        public string? department { get; set; } // Linked to tbl_office_type
        public int? region_id { get; set; }
        public int? province_id { get; set; }
        public string? company_name { get; set; }
        public bool? is_active { get; set; }
        public int createdBy { get; set; }
        public int modifiedBy { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
