using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreSections;

public record GetStoreSectionsQuery(
    Guid StoreId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<StoreSectionDto>>>, ICacheableQuery
{
    public string CacheKey => $"sections:store:{StoreId}:page:{PageNumber}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}