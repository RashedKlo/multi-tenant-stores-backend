using Application.Common.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return null;

        return new ProductDto(
            product.Id, product.DepartmentId, product.Title, product.Description,
            product.Price, product.DiscountPrice, product.Stock, product.ImageUrl,
            product.SKU, product.IsActive, product.CreatedAt, product.UpdatedAt
        );
    }
}
