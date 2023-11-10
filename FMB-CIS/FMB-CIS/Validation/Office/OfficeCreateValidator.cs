using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class OfficeCreateValidator : AbstractValidator<OfficeCreateViewModel>
    {
        public OfficeCreateValidator()
        {
            // Check name is not null, empty and is between 1 and 300 characters
            RuleFor(office => office.office_name).NotNull().NotEmpty().Length(1, 300);
            // Check description is not null, empty and is between 1 and 4000 characters
            RuleFor(office => office.department).NotNull(); //nvarcharmax
            // Check description is not null, empty and is between 1 and 4000 characters
            RuleFor(office => office.region_id).NotNull().NotEmpty().GreaterThan(0); //nvarcharmax
        }
    }
}