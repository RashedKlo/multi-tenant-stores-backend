using FluentValidation;

namespace Application.Addresses.Commands.DeleteAddress;

public class DeleteAddressValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("Id.Required")
            .WithMessage("Address Id must be a valid GUID.");
    }
}
