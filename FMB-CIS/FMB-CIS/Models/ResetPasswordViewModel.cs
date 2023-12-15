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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain at least 8 characters, including at least 1 uppercase letter, at least 1 lowercase letter, at least one number and a special character.")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match!")]
        public string ConfirmPassword { get; set; }
        public string tokencode { get; set; }
        public tbl_user_temp_passwords? tbl_User_Temp_Passwords { get; set; }
    }
}
