using FluentValidation;

namespace Application.Tenants.Commands.CreateTenant;

public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PasswordHash)
            .NotEmpty().WithMessage("Password hash is required.")
             .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters");

    }
}