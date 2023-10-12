using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match!")]
        public string ConfirmPassword { get; set; }
        public string tokencode { get; set; }
        public tbl_user_temp_passwords? tbl_User_Temp_Passwords { get; set; }
    }
}
