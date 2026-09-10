using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Queries.GetAddresses;

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

        var addresses = await repository.GetByCustomerIdAsync(
            customerId, cancellationToken);

        var dtos = addresses.Select(AddressDto.FromEntity).ToList();
        return Result<IReadOnlyList<AddressDto>>.Success(dtos);
    }
}
