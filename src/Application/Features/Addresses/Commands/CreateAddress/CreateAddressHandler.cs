// Application/Features/Addresses/Commands/CreateAddress/CreateAddressHandler.cs
using Application.Features.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.CreateAddress;

public sealed class CreateAddressHandler(
    ICustomerAddressRepository addresses,
    ICurrentUserService user)
    : IRequestHandler<CreateAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(CreateAddressCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result<AddressDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var createResult = CustomerAddress.Create(
            customerId,
            request.Label,
            request.Latitude,
            request.Longitude,
            request.AddressText,
            request.IsDefault);

        if (createResult.IsFailure)
            return Result<AddressDto>.Failure(createResult.Errors);

        var address = createResult.Value!;

        if (address.IsDefault)
        {
            var currentDefault = await addresses.GetDefaultForCustomerAsync(customerId, ct);
            if (currentDefault is not null)
            {
                var unset = currentDefault.UnsetDefault();
                if (unset.IsFailure)
                    return Result<AddressDto>.Failure(unset.Errors);
            }
        }

        await addresses.AddAsync(address, ct);
        await addresses.SaveChangesAsync(ct);

        return Result<AddressDto>.Success(AddressDto.FromEntity(address));
    }
}