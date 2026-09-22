using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreBanners;

public record GetStoreBannersQuery(Guid StoreId) : IRequest<Result<IReadOnlyList<StoreBannerDto>>>, ICacheableQuery
{
    public string CacheKey => $"banners:store:{StoreId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}