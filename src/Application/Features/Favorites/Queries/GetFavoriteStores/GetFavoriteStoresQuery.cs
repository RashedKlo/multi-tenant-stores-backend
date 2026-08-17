using Application.Common.Models;
using Application.Favorites.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Favorites.Queries.GetFavoriteStores;

public record GetFavoriteStoresQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<FavoriteStoreDto>>>;
