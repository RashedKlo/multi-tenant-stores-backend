using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Catalog.Queries.GetStoreById;

public class GetStoreByIdHandler(
    IStoreRepository storeRepository,
    IFavoriteStoreRepository favoriteStoreRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetStoreByIdQuery, Result<StoreDetailDto>>
{
    public async Task<Result<StoreDetailDto>> Handle(
        GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await storeRepository.GetByIdReadOnlyAsync(request.StoreId, cancellationToken);
        if (store is null || !store.IsActive || store.DeletedAt is not null)
            return Result<StoreDetailDto>.Failure(Error.NotFound("Store.NotFound", "Store not found"));

        var isFavorite = currentUser.IsAuthenticated
            && await favoriteStoreRepository.ExistsAsync(
                currentUser.CustomerId!.Value, store.Id, cancellationToken);

        var dto = StoreDetailDto.FromEntity(store, isFavorite);
        return Result<StoreDetailDto>.Success(dto);
    }
}
