using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductsBySection;

public record GetProductsBySectionQuery(
    Guid SectionId,
    bool? InStockOnly,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ProductSummaryDto>>>, ICacheableQuery
{
    public string CacheKey =>
        $"products:section:{SectionId}:instock:{InStockOnly?.ToString() ?? "any"}:min:{MinPrice?.ToString() ?? "none"}:max:{MaxPrice?.ToString() ?? "none"}:page:{PageNumber}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}