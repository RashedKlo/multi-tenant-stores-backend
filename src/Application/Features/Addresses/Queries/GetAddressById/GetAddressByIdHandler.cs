using Application.Features.Addresses.DTOs;
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
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<AddressDto>.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));
        var address = await repository.GetByIdForCustomerAsync(
            request.Id, currentUser.CustomerId!.Value, cancellationToken);
            
            if(address is null)
                return Result<AddressDto>.Failure(Error.NotFound("Address.NotFound", "Address not found"));

        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}
