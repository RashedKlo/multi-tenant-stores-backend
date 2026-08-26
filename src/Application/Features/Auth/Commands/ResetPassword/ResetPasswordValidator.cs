// Application/Auth/Commands/ResetPassword/ResetPasswordValidator.cs
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
                .WithMessage("Email is not a valid email address.");

        RuleFor(x => x.Code)
            .NotEmpty()
                .WithErrorCode("Code.Required")
                .WithMessage("Reset code is required.")
            .Matches(@"^\d{6}$")
                .WithErrorCode("Code.Invalid")
                .WithMessage("Reset code must be 6 digits.");

        // Align these rules with your Register validator — password policy
        // must be identical in both places. Consider a shared PasswordRule extension.
        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithErrorCode("Password.Required")
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithErrorCode("Password.TooShort")
                .WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]")
                .WithErrorCode("Password.Uppercase")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
                .WithErrorCode("Password.Lowercase")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d")
                .WithErrorCode("Password.Digit")
                .WithMessage("Password must contain at least one digit.");
    }
}
