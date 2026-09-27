using FluentValidation;

namespace Application.Addresses.Queries.GetDefaultAddress;

public class GetDefaultAddressValidator : AbstractValidator<GetDefaultAddressQuery>
{
    public GetDefaultAddressValidator() { }
}