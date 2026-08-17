using FluentValidation;

namespace Application.Auth.Commands.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithErrorCode("RefreshToken.Required")
            .WithMessage("Refresh token is required.");
    }
}
