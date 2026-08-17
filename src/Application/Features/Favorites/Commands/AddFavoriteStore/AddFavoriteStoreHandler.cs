using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Commands.AddFavoriteStore;

public class AddFavoriteStoreHandler(
    IFavoriteStoreRepository favoriteRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<AddFavoriteStoreCommand, Result>
{
    public async Task<Result> Handle(
        AddFavoriteStoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        if (await favoriteRepository.ExistsAsync(customerId, request.StoreId, cancellationToken))
            return Result.Success();

        var createResult = FavoriteStore.Create(customerId, request.StoreId);
        if (createResult.IsFailure)
            return Result.Failure(createResult.Errors);

        await favoriteRepository.AddAsync(createResult.Value!, cancellationToken);
        await favoriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
