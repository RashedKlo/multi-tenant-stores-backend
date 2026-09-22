using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetStoresByModule;

public record GetStoresByModuleQuery(
    Guid ModuleId,
    Guid? CategoryId,
    string? Search,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<StoreSummaryDto>>>, ICacheableQuery
{
    public string CacheKey =>
        $"stores:module:{ModuleId}:category:{CategoryId?.ToString() ?? "all"}:search:{Search ?? ""}:page:{PageNumber}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}