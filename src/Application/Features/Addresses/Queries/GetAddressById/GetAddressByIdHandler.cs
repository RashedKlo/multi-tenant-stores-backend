using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Queries.GetAddressById;

public class GetAddressByIdHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAddressByIdQuery, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        // Ownership-scoped by construction — a foreign address id simply
        // doesn't exist from this customer's point of view, so it 404s
        // rather than 403s. Don't leak whether the id exists at all.
        var address = await repository.GetByIdForCustomerAsync(
            request.Id, currentUser.CustomerId!.Value, cancellationToken);
            if(address is null)
                return Result<AddressDto>.Failure(Error.NotFound("Address.NotFound", "Address not found"));

        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}
