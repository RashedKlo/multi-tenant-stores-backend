using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "products:all";

    public GetAllProductsHandler(IProductRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetAsync<List<ProductDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var products = await _repository.GetAllAsync(cancellationToken);

        var dtos = products.Select(p => new ProductDto(
            p.Id, p.DepartmentId, p.Title, p.Description,
            p.Price, p.DiscountPrice, p.Stock, p.ImageUrl,
            p.SKU, p.IsActive, p.CreatedAt, p.UpdatedAt
        )).ToList();

        // Cache the result
        await _cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}
