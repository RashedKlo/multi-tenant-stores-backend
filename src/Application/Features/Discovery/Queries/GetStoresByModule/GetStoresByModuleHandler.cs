using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetStoresByModule;

public sealed class GetStoresByModuleHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoresByModuleQuery, Result<PagedResult<StoreSummaryDto>>>
{
    public async Task<Result<PagedResult<StoreSummaryDto>>> Handle(
        GetStoresByModuleQuery request,
        CancellationToken cancellationToken)
    {
        var result = await discoveryQueries.GetStoresByModuleAsync(
            request.ModuleId,
            request.CategoryId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<StoreSummaryDto>>.Success(result);
    }
}