namespace FMB_CIS.Models
{
    public class ViewModel
    {
        public IEnumerable<tbl_chainsaw>? tbl_Chainsaws { get; set; }
        public IEnumerable<ApplicationModel>? applicationModels { get; set; }
        public IEnumerable<tbl_user>? tbl_Users { get; set; }

        public IEnumerable<ApplicantListViewModel>? applicantListViewModels { get; set; }
        public string?    uid { get; set; }
        public string? appid { get; set; }
        public string? comment { get; set; }
        public string? email { get; set; }
        public tbl_application? tbl_Application { get; set; }
        public FileUpload? filesUpload { get; set; }

    }
}
