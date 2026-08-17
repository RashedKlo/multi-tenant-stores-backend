using FluentValidation;

namespace Application.Addresses.Queries.GetAddresses;

public class GetAddressesValidator : AbstractValidator<GetAddressesQuery>
{
    public GetAddressesValidator()
    {
        // No input parameters — customer is resolved from ICurrentUserService.
    }
}
