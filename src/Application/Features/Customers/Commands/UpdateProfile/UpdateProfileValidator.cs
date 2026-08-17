using FluentValidation;

namespace Application.Customers.Commands.UpdateProfile;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode("FirstName.Required")
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithErrorCode("FirstName.TooLong")
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode("LastName.Required")
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithErrorCode("LastName.TooLong")
            .WithMessage("Last name must not exceed 100 characters.");
    }
}
