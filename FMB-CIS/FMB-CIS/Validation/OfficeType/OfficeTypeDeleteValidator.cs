using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class OfficeTypeDeleteValidator : AbstractValidator<OfficeTypeDeleteViewModel>
    {
        public OfficeTypeDeleteValidator()
        {
            // Check od is not null, empty
            RuleFor(officeType => officeType.id).NotNull().NotEmpty();
        }
    }
}