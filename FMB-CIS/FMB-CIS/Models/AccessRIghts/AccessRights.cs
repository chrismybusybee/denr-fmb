using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class AccessRights
    {
        public int id { get; set; }
        public string code { get; set; } // CanAccessAll, CanAccessPageDashboard, CanEditUser, ReadOnlyAll...
        public string name { get; set; } // CanAccessAll, CanAccessPageDashboard, CanEditUser, ReadOnlyAll...
        public string description { get; set; }
        public string scope { get; set; } // For Visualization: Page, Module, Functionality
        public string type { get; set; } // For Visualization: RESTRICT, ALLOW
        public string parent_code { get; set; } // Note: A page can be parent to a page, same with module, and functionality, no restrictions as it is mostly for organization purpose
        public bool? is_active { get; set; }
        public int createdBy { get; set; }
        public int modifiedBy { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
