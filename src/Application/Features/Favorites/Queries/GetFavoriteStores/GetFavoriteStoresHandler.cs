using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Favorites.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Queries.GetFavoriteStores;

public class GetFavoriteStoresHandler(
    IFavoriteStoreRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetFavoriteStoresQuery, Result<PagedResult<FavoriteStoreDto>>>
{
    public async Task<Result<PagedResult<FavoriteStoreDto>>> Handle(
        GetFavoriteStoresQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<PagedResult<FavoriteStoreDto>>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var (items, totalCount) = await repository.GetPagedByCustomerIdAsync(
            currentUser.CustomerId.Value,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(FavoriteStoreDto.FromEntity).ToList();

        var result = PagedResult<FavoriteStoreDto>.Create(
            dtos,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<FavoriteStoreDto>>.Success(result);
    }
}
