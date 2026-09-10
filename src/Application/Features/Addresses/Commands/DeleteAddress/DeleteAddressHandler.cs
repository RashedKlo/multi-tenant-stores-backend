// Application/Features/Addresses/Commands/DeleteAddress/DeleteAddressHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.DeleteAddress;

public sealed class DeleteAddressHandler(
    ICustomerAddressRepository addresses,
    ICurrentUserService user)
    : IRequestHandler<DeleteAddressCommand, Result>
{
    public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var address = await addresses.GetByIdForCustomerAsync(request.Id, customerId, ct);
        if (address is null || address.IsDeleted)
            return Result.Failure(Error.NotFound("Address.NotFound", "Address not found."));

        var deleteResult = address.Delete();
        
        if (deleteResult.IsFailure)
            return deleteResult;

        await addresses.SaveChangesAsync(ct);
        return Result.Success();
    }
}