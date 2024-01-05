using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FMB_CIS.Models
{
    public class tbl_chainsaw
    {
        public int Id { get; set; }
        
        public int user_id { get; set; }
        public int? tbl_application_id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Engine { get; set; }
        public string? Power { get; set; }
        public string? remarks { get; set; }
        public string? status { get; set; }
        public int? watt { get; set; }
        public int? hp { get; set; }
        public string? gb { get; set; }
        public string? supplier { get; set; }
        public DateTime? date_purchase { get; set; }
        public bool? is_active { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public string? chainsaw_serial_number { get; set; }
        public DateTime? chainsaw_date_of_registration { get; set; }
        public DateTime? chainsaw_date_of_expiration { get; set; }


        public string? specification { get; set; }
        public string? purpose { get; set; }
    }
}
