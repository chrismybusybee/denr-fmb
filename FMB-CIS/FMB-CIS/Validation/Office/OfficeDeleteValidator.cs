using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class OfficeDeleteValidator : AbstractValidator<OfficeDeleteViewModel>
    {
        public OfficeDeleteValidator()
        {
            // Check od is not null, empty
            RuleFor(officeType => officeType.id).NotNull().NotEmpty();
        }
    }
}