namespace FMB_CIS.Models
{
    public class AcctApprovalViewModel
    {
        public string userType { get; set; }
        public int userID { get; set; }
        public string FullName { get; set; }
        public string email { get; set; }
        public string contact_no { get; set; }
        public string valid_id { get; set; }
        public string valid_id_no { get; set; }
        public string birth_date { get; set; }
        public string street_address {get; set; }
        public string region { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string brgy { get; set; }
        public bool status { get; set; }
        public string comment { get; set; }
        public DateTime? date_created { get; set; }
        
    }
}
