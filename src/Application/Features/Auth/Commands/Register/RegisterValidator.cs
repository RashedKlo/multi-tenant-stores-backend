using FluentValidation;

namespace Application.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode("FirstName.Required")
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithErrorCode("FirstName.TooLong")
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode("LastName.Required")
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithErrorCode("LastName.TooLong")
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode("Email.Required")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("Email.Invalid")
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256)
            .WithErrorCode("Email.TooLong")
            .WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode("Password.Required")
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithErrorCode("Password.TooShort")
            .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .WithErrorCode("Password.TooLong")
            .WithMessage("Password must not exceed 128 characters.")
            .Must(HaveRequiredComplexity)
            .WithErrorCode("Password.Weak")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");
    }

    private static bool HaveRequiredComplexity(string password) =>
        !string.IsNullOrEmpty(password)
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);
}
