using System.Net.Mail;
using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.CreateAddress;

public class CreateAddressHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var customerId = currentUser.CustomerId!.Value;
        var address = CustomerAddress.Create(
            customerId,
            request.Label,
            request.Latitude,
            request.Longitude,
            request.AddressText,
            request.IsDefault);
            if( address.IsFailure )
            {
                return Result<AddressDto>.Failure(Error.Validation("Address.Invalid", "Invalid address data"));
            }
        repository.Add(address.Value!);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Success(AddressDto.FromEntity(address.Value!));
    }
}
