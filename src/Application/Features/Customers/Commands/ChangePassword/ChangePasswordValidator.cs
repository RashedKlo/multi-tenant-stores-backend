using FluentValidation;

namespace Application.Customers.Commands.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithErrorCode("CurrentPassword.Required")
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode("NewPassword.Required")
            .WithMessage("New password is required.")
            .MinimumLength(8)
            .WithErrorCode("NewPassword.TooShort")
            .WithMessage("New password must be at least 8 characters.")
            .MaximumLength(128)
            .WithErrorCode("NewPassword.TooLong")
            .WithMessage("New password must not exceed 128 characters.")
            .Must(HaveRequiredComplexity)
            .WithErrorCode("NewPassword.Weak")
            .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, and one digit.");

        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword)
            .When(x => !string.IsNullOrEmpty(x.CurrentPassword) && !string.IsNullOrEmpty(x.NewPassword))
            .WithErrorCode("NewPassword.SameAsCurrent")
            .WithMessage("New password must be different from the current password.");
    }

    private static bool HaveRequiredComplexity(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit);
    }
}
