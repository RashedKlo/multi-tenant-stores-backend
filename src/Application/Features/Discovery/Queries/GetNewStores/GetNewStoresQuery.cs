using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetNewStores;

public record GetNewStoresQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<StoreSummaryDto>>>, ICacheableQuery
{
    public string CacheKey => $"stores:new:page:{PageNumber}:size:{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}