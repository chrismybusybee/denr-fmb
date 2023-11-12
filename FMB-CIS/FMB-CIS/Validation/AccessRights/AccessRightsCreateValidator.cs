using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class AccessRightsCreateValidator : AbstractValidator<AccessRightsCreateViewModel>
    {
        public AccessRightsCreateValidator()
        {
            RuleFor(accessRights => accessRights.code).NotNull().NotEmpty().Length(1, 300);
            RuleFor(accessRights => accessRights.name).NotNull().NotEmpty().Length(1, 300);
            RuleFor(accessRights => accessRights.description).NotNull().NotEmpty().Length(1, 4000);
            RuleFor(accessRights => accessRights.scope).NotNull().NotEmpty().Length(1, 300);
            RuleFor(accessRights => accessRights.type).NotNull().NotEmpty().Length(1, 300);
        }
    }
}