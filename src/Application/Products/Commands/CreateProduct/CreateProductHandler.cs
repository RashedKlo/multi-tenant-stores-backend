using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;

    public CreateProductHandler(IProductRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.DepartmentId,
            request.Title,
            request.Description,
            request.Price,
            request.Stock,
            request.ImageUrl,
            request.SKU
        );

        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"products:department:{request.DepartmentId}", cancellationToken);
        await _cache.RemoveAsync("products:all", cancellationToken);

        return new ProductDto(
            product.Id, product.DepartmentId, product.Title, product.Description,
            product.Price, product.DiscountPrice, product.Stock, product.ImageUrl,
            product.SKU, product.IsActive, product.CreatedAt, product.UpdatedAt
        );
    }
}
