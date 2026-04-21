using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Products.Queries.GetProductsByDepartmentId;

public class GetProductsByDepartmentIdHandler : IRequestHandler<GetProductsByDepartmentIdQuery, List<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;

    public GetProductsByDepartmentIdHandler(IProductRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByDepartmentIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:department:{request.DepartmentId}";

        // Try cache first
        var cached = await _cache.GetAsync<List<ProductDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var products = await _repository.GetByDepartmentIdAsync(request.DepartmentId, cancellationToken);

        var dtos = products.Select(p => new ProductDto(
            p.Id, p.DepartmentId, p.Title, p.Description,
            p.Price, p.DiscountPrice, p.Stock, p.ImageUrl,
            p.SKU, p.IsActive, p.CreatedAt, p.UpdatedAt
        )).ToList();

        // Cache the result
        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}
