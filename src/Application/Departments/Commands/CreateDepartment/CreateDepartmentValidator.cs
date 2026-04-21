using FluentValidation;

namespace Application.Departments.Commands.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(255).WithMessage("Department name must not exceed 255 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
