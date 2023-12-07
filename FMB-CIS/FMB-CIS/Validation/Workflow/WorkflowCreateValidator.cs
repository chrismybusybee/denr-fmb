using FluentValidation;
using FMB_CIS.Models;

namespace FMB_CIS.Validation
{
    public class WorkflowCreateValidator : AbstractValidator<WorkflowCreateViewModel>
    {
        public WorkflowCreateValidator()
        {
            RuleFor(workflow => workflow.permitType).NotNull().NotEmpty().Length(1, 300);
            RuleFor(workflow => workflow.code).NotNull().NotEmpty().Length(1, 300);
            RuleFor(workflow => workflow.name).NotNull().NotEmpty().Length(1, 300);
            RuleFor(workflow => workflow.description).NotNull().NotEmpty().Length(1, 4000);
            RuleFor(workflow => workflow.steps);
        }
    }
}