using Application.Addresses.Queries.GetAddresses;
using FluentValidation;

namespace Application.Features.Addresses.Queries.GetAddresses;

public class GetAddressesValidator : AbstractValidator<GetAddressesQuery>
{
    public GetAddressesValidator()
    {
        // No input parameters — customer is resolved from ICurrentUserService.
    }
}
