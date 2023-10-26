namespace FMB_CIS.Models
{
    public class tbl_user_change_info_request_status
    {
        public int id { get; set; }
        public string? status_name { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public int? date_created { get; set; }
        public int? date_modified { get; set; }
                
        //1	Approved
        //2	Pending
        //3	Declined
    }
}
