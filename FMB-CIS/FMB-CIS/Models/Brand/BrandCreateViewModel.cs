using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class BrandCreateViewModel
    {
        public string name { get; set; } // Stihl, Makita, Hitachi
        public string description { get; set; }
    }
}
