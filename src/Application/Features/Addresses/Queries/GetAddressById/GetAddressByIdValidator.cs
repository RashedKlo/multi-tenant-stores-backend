using FluentValidation;

namespace Application.Addresses.Queries.GetAddressById;

public class GetAddressByIdValidator : AbstractValidator<GetAddressByIdQuery>
{
    public GetAddressByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("Id.Required")
            .WithMessage("Address Id must be a valid GUID.");
    }
}
