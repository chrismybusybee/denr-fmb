using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class UserTypeUser
    {
        public int id { get; set; }
        public int user_type_id { get; set; }
        public int user_id { get; set; }
        public bool? is_active { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
