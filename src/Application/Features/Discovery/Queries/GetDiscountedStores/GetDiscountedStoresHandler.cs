using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetDiscountedStores;

public sealed class GetDiscountedStoresHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetDiscountedStoresQuery, Result<PagedResult<DiscountedStoreDto>>>
{
    public async Task<Result<PagedResult<DiscountedStoreDto>>> Handle(
        GetDiscountedStoresQuery request,
        CancellationToken cancellationToken)
    {
        var stores = await discoveryQueries.GetDiscountedStoresAsync(
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<DiscountedStoreDto>>.Success(stores);
    }
}