using FluentValidation;

namespace Application.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(255).WithMessage("Department name must not exceed 255 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
