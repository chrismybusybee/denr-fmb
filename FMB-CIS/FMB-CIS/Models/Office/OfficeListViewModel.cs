using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class OfficeListViewModel
    {
        public IEnumerable<Office> offices { get; set; }
    }
}
