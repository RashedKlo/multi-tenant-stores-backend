using FluentValidation;

namespace Application.Addresses.Commands.UpdateAddress;

public class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("Id.Required")
            .WithMessage("Address Id must be a valid GUID.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithErrorCode("Label.Required")
            .WithMessage("Label is required.")
            .MaximumLength(100)
            .WithErrorCode("Label.TooLong")
            .WithMessage("Label must not exceed 100 characters.");

        RuleFor(x => x.AddressText)
            .NotEmpty()
            .WithErrorCode("AddressText.Required")
            .WithMessage("AddressText is required.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithErrorCode("Latitude.Invalid")
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithErrorCode("Longitude.Invalid")
            .WithMessage("Longitude must be between -180 and 180.");
    }
}
