using FluentValidation;

namespace Application.Auth.Commands.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode("Email.Required")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("Email.Invalid")
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("Code.Required")
            .WithMessage("Reset code is required.")
            .Length(6)
            .WithErrorCode("Code.InvalidLength")
            .WithMessage("Reset code must be 6 digits.");

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
    }

    private static bool HaveRequiredComplexity(string password) =>
        !string.IsNullOrEmpty(password)
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);
}
