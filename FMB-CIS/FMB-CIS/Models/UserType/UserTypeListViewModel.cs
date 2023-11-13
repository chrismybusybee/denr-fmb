using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserTypeListViewModel
    {
        public IEnumerable<UserType> userTypes { get; set; }
    }
}
