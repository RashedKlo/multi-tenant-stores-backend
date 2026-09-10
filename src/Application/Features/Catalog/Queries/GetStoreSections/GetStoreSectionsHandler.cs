using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreSections;

public sealed class GetStoreSectionsHandler(
    ICatalogQueries catalogQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoreSectionsQuery, Result<PagedResult<StoreSectionDto>>>
{
    public async Task<Result<PagedResult<StoreSectionDto>>> Handle(
        GetStoreSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await catalogQueries.GetStoreSectionsAsync(
            request.StoreId,
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<StoreSectionDto>>.Success(result);
    }
}
