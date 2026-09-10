using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductById;

public sealed class GetProductByIdHandler(
    ICatalogQueries catalogQueries,
    ICurrentUserService currentUser,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await catalogQueries.GetProductByIdAsync(
            request.ProductId,
            currentUser.CustomerId,
            currentLanguageProvider.Language,
            cancellationToken);

        return product is null
            ? Result<ProductDetailDto>.Failure(Error.NotFound("Product.NotFound", "Product not found."))
            : Result<ProductDetailDto>.Success(product);
    }
}
