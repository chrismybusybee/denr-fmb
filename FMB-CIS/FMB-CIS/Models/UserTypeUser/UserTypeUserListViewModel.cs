using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserTypeUserListViewModel
    {
        public IEnumerable<User> users{ get; set; }
        public IEnumerable<UserTypeUser> userTypeUsers { get; set; }
        public IEnumerable<UserType> userTypes { get; set; }
    }
}
