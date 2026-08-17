using Application.Catalog.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using Application.Common.Interfaces;
namespace Application.Catalog.Queries.GetProductById;

public class GetProductByIdHandler(
    IProductRepository productRepository,
    IFavoriteProductRepository favoriteProductRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(
        GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // GetByIdWithDetailsAsync already filters IsActive/DeletedAt and uses
        // AsSplitQuery — see ProductRepository, built in the Infrastructure pass.
        var product = await productRepository.GetByIdWithDetailsAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive || product.DeletedAt is not null)
            return Result<ProductDetailDto>.Failure(Error.NotFound("Product.NotFound", "Product not found"));

        var isFavorite = currentUser.IsAuthenticated
            && await favoriteProductRepository.ExistsAsync(
                currentUser.CustomerId!.Value, product.Id, cancellationToken);

        var dto = ProductDetailDto.FromEntity(product, isFavorite);
        return Result<ProductDetailDto>.Success(dto);
    }
}
