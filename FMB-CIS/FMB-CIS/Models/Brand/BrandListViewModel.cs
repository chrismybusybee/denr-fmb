using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class BrandListViewModel
    {
        public IEnumerable<Brand> brands { get; set; }
    }
}
