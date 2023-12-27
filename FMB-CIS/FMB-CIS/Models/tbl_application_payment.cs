namespace FMB_CIS.Models
{
    public class tbl_application_payment
    {
        public int id { get; set; }
        public int? tbl_application_id { get; set; }
        public string? OR_Number { get; set; }
        public DateTime? Date_of_Payment { get; set; }
        public double? Amount { get; set; }
        public bool? allow_edit { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
