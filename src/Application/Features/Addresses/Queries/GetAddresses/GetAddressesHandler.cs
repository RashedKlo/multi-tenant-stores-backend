using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Queries.GetAddresses;

public class GetAddressesHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAddressesQuery, Result<List<AddressDto>>>
{
    public async Task<Result<List<AddressDto>>> Handle(
        GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await repository.GetByCustomerIdAsync(
            currentUser.CustomerId!.Value, cancellationToken);

        var dtos = addresses.Select(AddressDto.FromEntity).ToList();
        return Result<List<AddressDto>>.Success(dtos);
    }
}
