using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid StoreId) : IRequest<Result<StoreDetailDto>>, ICacheableQuery
{
    public string CacheKey => $"store:{StoreId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
