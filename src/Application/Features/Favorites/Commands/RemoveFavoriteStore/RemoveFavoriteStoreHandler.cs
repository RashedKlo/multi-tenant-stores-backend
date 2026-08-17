using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Commands.RemoveFavoriteStore;

public class RemoveFavoriteStoreHandler(
    IFavoriteStoreRepository favoriteRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<RemoveFavoriteStoreCommand, Result>
{
    public async Task<Result> Handle(
        RemoveFavoriteStoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        var favorite = await favoriteRepository.GetAsync(customerId, request.StoreId, cancellationToken);
        if (favorite is null)
            return Result.Failure(Error.NotFound("FavoriteStore.NotFound", "Favorite store not found"));

        favoriteRepository.Delete(favorite);
        await favoriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
