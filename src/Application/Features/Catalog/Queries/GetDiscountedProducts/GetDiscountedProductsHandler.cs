using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetDiscountedProducts;

public sealed class GetDiscountedProductsHandler(
    ICatalogQueries catalogQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetDiscountedProductsQuery, Result<PagedResult<ProductSummaryDto>>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(
        GetDiscountedProductsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await catalogQueries.GetDiscountedProductsAsync(
            storeId: request.StoreId,
            page: request.PageNumber,
            pageSize: request.PageSize,
            lang: currentLanguageProvider.Language,
            ct: cancellationToken);

        return Result<PagedResult<ProductSummaryDto>>.Success(result);
    }
}
