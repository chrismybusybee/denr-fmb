using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class ChangePasswordViewModel
    {
        //[Required]
        //[EmailAddress]
        //public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Required]
        [Display(Name = "Confirm Old Password")]
        [Compare("OldPassword", ErrorMessage = "Old Password and Confirm OldPassword must match!")]
        public string ConfirmOldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm New Password must match!")]
        public string ConfirmNewPassword { get; set; }
        public bool? isSuccess { get; set; }
    }
}
