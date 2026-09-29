using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetDiscountedStores;

public record GetDiscountedStoresQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<DiscountedStoreDto>>>, ICacheableQuery
{
    public string CacheKey => $"stores:discounted:page:{PageNumber}:size:{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}