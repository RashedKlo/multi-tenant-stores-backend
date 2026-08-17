using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Addresses.Commands.DeleteAddress;

public class DeleteAddressHandler(
    ICustomerAddressRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteAddressCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var customerId = currentUser.CustomerId!.Value;

        var address = await repository.GetByIdForCustomerAsync(request.Id, customerId, cancellationToken);
        if (address is null 
        || address.IsDeleted  )
            return Result<bool>.Success(false);

        address.Delete();
        repository.Update(address);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
