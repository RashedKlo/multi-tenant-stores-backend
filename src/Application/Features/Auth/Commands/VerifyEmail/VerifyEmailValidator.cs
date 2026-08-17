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
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("Code.Required")
            .WithMessage("Verification code is required.")
            .Length(6)
            .WithErrorCode("Code.InvalidLength")
            .WithMessage("Verification code must be 6 digits.");
    }
}
