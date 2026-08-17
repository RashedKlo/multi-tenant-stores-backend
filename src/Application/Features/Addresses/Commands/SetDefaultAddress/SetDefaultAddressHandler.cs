using Application.Addresses.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.SetDefaultAddress;

public class SetDefaultAddressHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<SetDefaultAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var customerId = currentUser.CustomerId!.Value;

        var target = await repository.GetByIdForCustomerAsync(request.Id, customerId, cancellationToken);
        if (target is null || target.IsDeleted)
            return Result<AddressDto>.Failure(Error.NotFound("Address.NotFound", "Address not found"));
        if (target.IsDefault)
            return Result<AddressDto>.Success(AddressDto.FromEntity(target)); // already default — no-op

        target.SetAsDefault();
        repository.Update(target);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Success(AddressDto.FromEntity(target));
    }
}
