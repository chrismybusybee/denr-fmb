using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models.ManageProfile
{
    public class ManageProfileViewModel
    {
        //From tbl_user
        public tbl_user? tbl_User { get; set; }

        //Change Password
        [Required]
        [DataType(DataType.Password)]        
        public string OldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "New Password must contain at least 8 characters, including at least 1 uppercase letter, at least 1 lowercase letter, at least one number and a special character.")]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password must match!")]
        public string ConfirmPassword { get; set; }
    }
}
