using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetNearByStores;

public record GetNearByStoresQuery(
    decimal Latitude,
    decimal Longitude,
    int RadiusKm = 10,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<NearbyStoreDto>>>, ICacheableQuery
{
    public string CacheKey =>
        $"stores:nearby:lat:{Latitude}:lng:{Longitude}:radius:{RadiusKm}:page:{PageNumber}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}