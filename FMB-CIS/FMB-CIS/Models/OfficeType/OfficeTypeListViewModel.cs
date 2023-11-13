using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class OfficeTypeListViewModel
    {
        public IEnumerable<OfficeType> officeTypes { get; set; }
    }
}
