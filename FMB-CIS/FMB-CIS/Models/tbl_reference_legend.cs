namespace FMB_CIS.Models
{
    public class tbl_reference_legend
    {
        public int id { get; set; }
        public int permit_type_id { get; set; }
        public string legend { get; set; }
        public int created_by { get; set; }
        public int modified_by { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
    }
}
