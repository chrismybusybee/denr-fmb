using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class AccessRightsListViewModel
    {
        public IEnumerable<AccessRights> accessRights { get; set; }
    }
}
