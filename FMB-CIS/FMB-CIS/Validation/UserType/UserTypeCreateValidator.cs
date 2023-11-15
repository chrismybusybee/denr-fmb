using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class UserTypeCreateValidator : AbstractValidator<UserTypeCreateViewModel>
    {
        public UserTypeCreateValidator()
        {
            RuleFor(accessRights => accessRights.name).NotNull().NotEmpty().Length(1, 300);
        }
    }
}