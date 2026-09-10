// Application/Features/Addresses/Commands/SetDefaultAddress/SetDefaultAddressHandler.cs
using Application.Features.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressHandler(
    ICustomerAddressRepository addresses,
    ICurrentUserService user)
    : IRequestHandler<SetDefaultAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(SetDefaultAddressCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result<AddressDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var address = await addresses.GetByIdForCustomerAsync(request.Id, customerId, ct);
        if (address is null || address.IsDeleted)
            return Result<AddressDto>.Failure(
                Error.NotFound("Address.NotFound", "Address not found."));

        if (address.IsDefault)
            return Result<AddressDto>.Success(AddressDto.FromEntity(address)); // idempotent

        var currentDefault = await addresses.GetDefaultForCustomerAsync(customerId, ct);
        if (currentDefault is not null)
        {
            var unset = currentDefault.UnsetDefault();
            if (unset.IsFailure)
                return Result<AddressDto>.Failure(unset.Errors);
        }

        var set = address.SetAsDefault();
        if (set.IsFailure)
            return Result<AddressDto>.Failure(set.Errors);

        await addresses.SaveChangesAsync(ct);
        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}