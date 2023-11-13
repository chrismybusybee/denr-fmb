using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class OfficeTypeUpdateViewModel
    {
        public int id { get; set; }
        public string name { get; set; } // PENRO, CENRO, RED
        public string description { get; set; }
    }
}
