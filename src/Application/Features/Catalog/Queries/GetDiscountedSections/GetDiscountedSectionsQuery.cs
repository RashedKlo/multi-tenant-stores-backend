using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetDiscountedSections;

public record GetDiscountedSectionsQuery(
    Guid StoreId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<StoreSectionDto>>>, ICacheableQuery
{
    public string CacheKey => $"sections:store:{StoreId}:discounted:page:{PageNumber}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
}
