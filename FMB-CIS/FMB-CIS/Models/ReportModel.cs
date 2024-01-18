namespace FMB_CIS.Models
{
    public class ReportModel
    {
        public string? ReferenceNo { get;set; }

        public int? id { get;set; }

        public DateTime? applicationDate { get;set; }

        public int? qty { get; set; }

        public string full_name { get; set;}

        public string? email { get; set; }

        public string contact { get; set; }  
        
        public string address { get; set; }

        public string application_type { get; set; }

        public string permit_type { get; set; }

        public string permit_status { get; set; }

        public int tbl_user_id { get; set; }

        public DateTime? date_due_for_officers { get; set; }

        public bool? isRead { get; set; }

        public int? currentStepCount { get; set; }

        public int? currentMaxCount { get; set;}
    }
}
