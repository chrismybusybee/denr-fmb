using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class UserTypeUpdateValidator : AbstractValidator<UserTypeUpdateViewModel>
    {
        public UserTypeUpdateValidator()
        {
            RuleFor(accessRights => accessRights.id).NotNull().NotEmpty();
            RuleFor(accessRights => accessRights.name).NotNull().NotEmpty().Length(1, 300);
        }
    }
}