using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
    }
}
