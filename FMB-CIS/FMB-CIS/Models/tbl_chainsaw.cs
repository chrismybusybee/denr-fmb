using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class tbl_chainsaw
    {
        public int Id { get; set; }

        public int user_id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Engine { get; set; }
        public string? Power { get; set; }

        public string? status { get; set; }

    }
}
