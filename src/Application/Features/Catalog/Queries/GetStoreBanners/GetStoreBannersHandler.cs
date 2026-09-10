using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreBanners;

public sealed class GetStoreBannersHandler(
    ICatalogQueries catalogQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoreBannersQuery, Result<IReadOnlyList<StoreBannerDto>>>
{
    public async Task<Result<IReadOnlyList<StoreBannerDto>>> Handle(
        GetStoreBannersQuery request,
        CancellationToken cancellationToken)
    {
        var banners = await catalogQueries.GetStoreBannersAsync(
            request.StoreId,
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<IReadOnlyList<StoreBannerDto>>.Success(banners);
    }
}
