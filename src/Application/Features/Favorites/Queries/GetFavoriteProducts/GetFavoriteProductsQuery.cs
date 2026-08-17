using Application.Common.Models;
using Application.Favorites.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Favorites.Queries.GetFavoriteProducts;

public record GetFavoriteProductsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<FavoriteProductDto>>>;
