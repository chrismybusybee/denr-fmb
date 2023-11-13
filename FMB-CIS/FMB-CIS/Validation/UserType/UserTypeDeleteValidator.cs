using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class UserTypeDeleteValidator : AbstractValidator<UserTypeDeleteViewModel>
    {
        public UserTypeDeleteValidator()
        {
            RuleFor(accessRights => accessRights.id).NotNull().NotEmpty();
        }
    }
}