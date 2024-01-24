namespace FMB_CIS.Models
{
    public class tbl_email_template
    {
        public int id {  get; set; }
        public string? template_name { get; set; }
        public string? template_description { get; set; }
        public string? email_subject { get; set; }
        public string? email_content { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
