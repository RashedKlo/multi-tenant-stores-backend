using FluentValidation;

namespace Application.Auth.Commands.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode("Email.Required")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("Email.Invalid")
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode("Password.Required")
            .WithMessage("Password is required.");

        RuleFor(x => x.StoreId)
            .Must(id => id is null || id != Guid.Empty)
            .WithErrorCode("StoreId.Invalid")
            .WithMessage("StoreId must be a valid GUID when provided.");
    }
}
