using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;

    public UpdateProductHandler(IProductRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return null;

        product.Update(request.Title, request.Description, request.Price, request.DiscountPrice, request.Stock, request.ImageUrl);
        _repository.Update(product);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"products:department:{product.DepartmentId}", cancellationToken);
        await _cache.RemoveAsync("products:all", cancellationToken);

        return new ProductDto(
            product.Id, product.DepartmentId, product.Title, product.Description,
            product.Price, product.DiscountPrice, product.Stock, product.ImageUrl,
            product.SKU, product.IsActive, product.CreatedAt, product.UpdatedAt
        );
    }
}
