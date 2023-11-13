using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class UserTypeAccessRightsToggleValidator : AbstractValidator<UserTypeAccessRightsToggleViewModel>
    {
        public UserTypeAccessRightsToggleValidator()
        {
            RuleFor(userTypeAccessRight => userTypeAccessRight.userTypeId).NotNull();
            RuleFor(userTypeAccessRight => userTypeAccessRight.accessRightsId).NotNull();
        }
    }
}