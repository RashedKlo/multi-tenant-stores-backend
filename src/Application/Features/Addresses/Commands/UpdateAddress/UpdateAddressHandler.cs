using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.UpdateAddress;

// Deliberately has no IsDefault parameter — changing which address is
// default is a distinct action (SetDefaultAddressCommand) with its own
// invariant to maintain, not a side effect of an unrelated field edit.
public class UpdateAddressHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await repository.GetByIdForCustomerAsync(
            request.Id, currentUser.CustomerId!.Value, cancellationToken);
        if (address is null || address.IsDeleted)
            return Result<AddressDto>.Failure(Error.NotFound("Address.NotFound", "Address not found"));

        address.Update(request.Label, request.Latitude, request.Longitude, request.AddressText);

        repository.Update(address);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}
