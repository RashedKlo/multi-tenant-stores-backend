// Application/Features/Addresses/Commands/UpdateAddress/UpdateAddressHandler.cs
using Application.Features.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.UpdateAddress;

public sealed class UpdateAddressHandler(
    ICustomerAddressRepository addresses,
    ICurrentUserService user)
    : IRequestHandler<UpdateAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result<AddressDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var address = await addresses.GetByIdForCustomerAsync(request.Id, customerId, ct);
        if (address is null || address.IsDeleted)
            return Result<AddressDto>.Failure(
                Error.NotFound("Address.NotFound", "Address not found."));

        var updateResult = address.Update(
            request.Label,
            request.Latitude,
            request.Longitude,
            request.AddressText);

        if (updateResult.IsFailure)
            return Result<AddressDto>.Failure(updateResult.Errors);

        await addresses.SaveChangesAsync(ct);
        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}