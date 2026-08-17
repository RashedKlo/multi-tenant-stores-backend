using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Commands.RemoveFavoriteProduct;

public class RemoveFavoriteProductHandler(
    IFavoriteProductRepository favoriteRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<RemoveFavoriteProductCommand, Result>
{
    public async Task<Result> Handle(
        RemoveFavoriteProductCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        var favorite = await favoriteRepository.GetAsync(customerId, request.ProductId, cancellationToken);
        if (favorite is null)
            return Result.Success(); // already not favorited — idempotent

        favoriteRepository.Delete(favorite);
        await favoriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
