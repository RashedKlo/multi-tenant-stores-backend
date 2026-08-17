using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Commands.AddFavoriteProduct;

public class AddFavoriteProductHandler(
    IFavoriteProductRepository favoriteRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<AddFavoriteProductCommand, Result>
{
    public async Task<Result> Handle(
        AddFavoriteProductCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        // Idempotent — favoriting something already-favorited is a no-op
        // success, not an error. A double-tap on the heart icon should never surface a failure.
        if (await favoriteRepository.ExistsAsync(customerId, request.ProductId, cancellationToken))
            return Result.Success();

        var createResult = FavoriteProduct.Create(customerId, request.ProductId);
        if (createResult.IsFailure)
            return Result.Failure(createResult.Errors);

        await favoriteRepository.AddAsync(createResult.Value!, cancellationToken);
        await favoriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
