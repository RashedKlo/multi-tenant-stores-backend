using Application.Features.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using Application.Addresses.Queries.GetAddresses;

namespace Application.Features.Addresses.Queries.GetAddresses;

public class GetAddressesHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAddressesQuery, Result<IReadOnlyList<AddressDto>>>
{
    public async Task<Result<IReadOnlyList<AddressDto>>> Handle(
        GetAddressesQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<IReadOnlyList<AddressDto>>.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));
        var customerId = currentUser.CustomerId.Value;

        var addresses = await repository.GetActiveByCustomerAsync(
            customerId, cancellationToken);

        var dtos = addresses.Select(AddressDto.FromEntity).ToList();
        return Result<IReadOnlyList<AddressDto>>.Success(dtos);
    }
}
