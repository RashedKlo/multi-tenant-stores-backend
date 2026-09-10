using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductsBySection;

public sealed class GetProductsBySectionHandler(
    ICatalogQueries catalogQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetProductsBySectionQuery, Result<PagedResult<ProductSummaryDto>>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(
        GetProductsBySectionQuery request,
        CancellationToken cancellationToken)
    {
        var result = await catalogQueries.GetProductsBySectionAsync(
            request.SectionId,
            request.InStockOnly,
            request.MinPrice,
            request.MaxPrice,
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<ProductSummaryDto>>.Success(result);
    }
}
