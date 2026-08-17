using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Favorites.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Favorites.Queries.GetFavoriteProducts;

public class GetFavoriteProductsHandler(
    IFavoriteProductRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetFavoriteProductsQuery, Result<PagedResult<FavoriteProductDto>>>
{
    public async Task<Result<PagedResult<FavoriteProductDto>>> Handle(
        GetFavoriteProductsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<PagedResult<FavoriteProductDto>>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var (items, totalCount) = await repository.GetPagedByCustomerIdAsync(
            currentUser.CustomerId.Value,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(FavoriteProductDto.FromEntity).ToList();

        var result = PagedResult<FavoriteProductDto>.Create(
            dtos,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<FavoriteProductDto>>.Success(result);
    }
}
