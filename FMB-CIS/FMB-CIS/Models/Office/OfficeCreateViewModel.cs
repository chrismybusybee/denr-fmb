using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class OfficeCreateViewModel
    {
        public string? office_name { get; set; }
        public int? department { get; set; } // Linked to tbl_office_type
        public int? region_id { get; set; } = 0;
        public int? province_id { get; set; } = 0;
        public int? city_id { get; set; } = 0;
        public string? company_name { get; set; }
    }
}
