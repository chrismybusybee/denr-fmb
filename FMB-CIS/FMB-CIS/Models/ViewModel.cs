namespace FMB_CIS.Models
{
    public class ViewModel
    {
        public IEnumerable<tbl_chainsaw>? tbl_Chainsaws { get; set; }
        public IEnumerable<ApplicationModel>? applicationModels { get; set; }
        public IEnumerable<tbl_user>? tbl_Users { get; set; }

        public IEnumerable<ApplicantListViewModel>? applicantListViewModels { get; set; }

        public ApplicantListViewModel? applicantViewModels { get; set; }
        public IEnumerable<tbl_files>? tbl_Files { get; set; }
        public string?    uid { get; set; }
        public string? appid { get; set; }
        public string? comment { get; set; }
        public string? email { get; set; }
        public tbl_application? tbl_Application { get; set; }
        public FileUpload? filesUpload { get; set; }
        public IEnumerable<AcctApprovalViewModel>? acctList { get; set; } //Used in the List of Accounts
        public AcctApprovalViewModel? acctApprovalViewModels { get; set; } // Used in the Accounts Approval Page

        public tbl_user? tbl_User { get; set; } //Not IEnumerable and used in Editing of Account
        public IEnumerable<tbl_region>? tbl_Regions { get; set; }
        public IEnumerable<tbl_province>? tbl_Provinces { get; set; }
        public IEnumerable<tbl_city>? tbl_Cities { get; set; }
        public IEnumerable<tbl_brgy>? tbl_Brgys { get; set; }



    }
}
