using FluentValidation;

namespace Application.Auth.Commands.GoogleLogin;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithErrorCode("IdToken.Required")
            .WithMessage("Google ID token is required.");

    }
}
