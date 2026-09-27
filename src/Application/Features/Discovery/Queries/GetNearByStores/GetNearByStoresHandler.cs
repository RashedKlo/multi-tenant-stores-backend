using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetNearByStores;

public sealed class GetNearByStoresHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetNearByStoresQuery, Result<PagedResult<NearbyStoreDto>>>
{
    public async Task<Result<PagedResult<NearbyStoreDto>>> Handle(
        GetNearByStoresQuery request,
        CancellationToken cancellationToken)
    {
        var stores = await discoveryQueries.GetNearbyStoresAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            request.PageNumber,
            request.PageSize,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<PagedResult<NearbyStoreDto>>.Success(stores);
    }
}