using Application.Common.Interfaces;
using Application.Features.Addresses.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Queries.GetDefaultAddress;

public class GetDefaultAddressHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetDefaultAddressQuery, Result<DefaultAddressDto>>
{
    public async Task<Result<DefaultAddressDto>> Handle(
        GetDefaultAddressQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<DefaultAddressDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var address = await repository.GetDefaultForCustomerAsync(
            currentUser.CustomerId.Value, cancellationToken);

        if (address is null)
            return Result<DefaultAddressDto>.Failure(
                Error.NotFound("Address.NotFound", "Address not found"));

        return Result<DefaultAddressDto>.Success(
            new DefaultAddressDto(
                address.Id,
                address.Latitude,
                address.Longitude));
    }
}