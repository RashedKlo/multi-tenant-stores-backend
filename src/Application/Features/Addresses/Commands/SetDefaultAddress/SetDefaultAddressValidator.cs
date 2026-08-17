using FluentValidation;

namespace Application.Addresses.Commands.SetDefaultAddress;

public class SetDefaultAddressValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("Id.Required")
            .WithMessage("Address Id must be a valid GUID.");
    }
}
