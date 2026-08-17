using FluentValidation;

namespace Application.Auth.Commands.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithErrorCode("RefreshToken.Required")
            .WithMessage("Refresh token is required.");
    }
}
