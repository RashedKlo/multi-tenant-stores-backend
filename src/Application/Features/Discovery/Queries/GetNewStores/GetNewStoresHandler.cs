using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetNewStores;

public sealed class GetNewStoresHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetNewStoresQuery, Result<PagedResult<StoreSummaryDto>>>
{
    public async Task<Result<PagedResult<StoreSummaryDto>>> Handle(
        GetNewStoresQuery request,
        CancellationToken cancellationToken)
    {
        var stores = await discoveryQueries.GetNewStoresAsync(
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<StoreSummaryDto>>.Success(stores);
    }
}