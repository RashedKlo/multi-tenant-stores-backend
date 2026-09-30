using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetDiscountedSections;

public sealed class GetDiscountedSectionsHandler(
    ICatalogQueries catalogQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetDiscountedSectionsQuery, Result<PagedResult<StoreSectionDto>>>
{
    public async Task<Result<PagedResult<StoreSectionDto>>> Handle(
        GetDiscountedSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await catalogQueries.GetStoreSectionsAsync(
            storeId: request.StoreId,
            discountedOnly: true,
            page: request.PageNumber,
            pageSize: request.PageSize,
            lang: currentLanguageProvider.Language,
            ct: cancellationToken);

        return Result<PagedResult<StoreSectionDto>>.Success(result);
    }
}
