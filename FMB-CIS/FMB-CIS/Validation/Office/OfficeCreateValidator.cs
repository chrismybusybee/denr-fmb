using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class OfficeCreateValidator : AbstractValidator<OfficeCreateViewModel>
    {
        public OfficeCreateValidator()
        {
            // Check name is not null, empty and is between 1 and 300 characters
            RuleFor(officeType => officeType.office_name).NotNull().NotEmpty().Length(1, 300);
            // Check description is not null, empty and is between 1 and 4000 characters
            //RuleFor(officeType => officeType.description).NotNull().NotEmpty().Length(1, 4000); //nvarcharmax
        }
    }
}