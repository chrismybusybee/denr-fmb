using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class BrandUpdateViewModel
    {
        public int id { get; set; }
        public string name { get; set; } // Stihl, Makita, Hitachi
        public string description { get; set; }
    }
}
