// Application/Auth/Commands/VerifyEmail/VerifyEmailValidator.cs
using FluentValidation;

namespace Application.Auth.Commands.VerifyEmail;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
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
                .WithMessage("Verification code is required.")
            .Matches(@"^\d{6}$")
                .WithErrorCode("Code.Invalid")
                .WithMessage("Verification code must be 6 digits.");
    }
}
