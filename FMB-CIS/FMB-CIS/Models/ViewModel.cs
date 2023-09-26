namespace FMB_CIS.Models
{
    public class ViewModel
    {
        public IEnumerable<tbl_chainsaw>? tbl_Chainsaws { get; set; }
        public IEnumerable<ApplicationModel>? applicationModels { get; set; }
        public ChainsawSeller? chainsawSeller { get; set; }
        public tbl_application? tbl_Application { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
        public IEnumerable<tbl_user>? tbl_Users { get; set; }

=======
>>>>>>> Updated dashboard for temporary Cenro User, added application for permits.
=======
        public IEnumerable<tbl_user>? tbl_Users { get; set; }

>>>>>>> updated approval for permits applications
    }
}
