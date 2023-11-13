using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserTypeAccessRightsListViewModel
    {
        public IEnumerable<AccessRights> accessRights { get; set; }
        public IEnumerable<UserTypeAccessRights> userTypeAccessRights { get; set; }
        public IEnumerable<UserType> userTypes { get; set; }
    }
}
