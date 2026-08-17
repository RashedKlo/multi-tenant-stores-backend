using FluentValidation;

namespace Application.Auth.Commands.ResendVerification;

public class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode("Email.Required")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("Email.Invalid")
            .WithMessage("Email must be a valid email address.");
    }
}
