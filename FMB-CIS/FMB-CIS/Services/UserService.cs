
using FMB_CIS.Data;

namespace Services.Services
{
    public class UserService
    {
        private LocalContext _context { get; set; }
        public UserService(LocalContext context)
        {
            _context = context;
        }
        public bool IsAdminActivated(int uid)
        {
            bool? userStatus = _context.tbl_user.Where(u => u.id == uid).Select(u => u.status).SingleOrDefault();
            return (bool)userStatus;
        }
    }
}