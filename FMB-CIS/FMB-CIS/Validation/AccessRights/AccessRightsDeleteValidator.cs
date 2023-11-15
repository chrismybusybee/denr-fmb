using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class AccessRightsDeleteValidator : AbstractValidator<AccessRightsDeleteViewModel>
    {
        public AccessRightsDeleteValidator()
        {
            RuleFor(accessRights => accessRights.id).NotNull().NotEmpty();
        }
    }
}