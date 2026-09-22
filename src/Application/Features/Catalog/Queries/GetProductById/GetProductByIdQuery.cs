using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductById;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDetailDto>>, ICacheableQuery
{
    public string CacheKey => $"product:{ProductId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
