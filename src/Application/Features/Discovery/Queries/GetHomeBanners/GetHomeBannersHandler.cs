using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetHomeBanners;

public sealed class GetHomeBannersHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetHomeBannersQuery, Result<IReadOnlyList<HomeBannerDto>>>
{
    public async Task<Result<IReadOnlyList<HomeBannerDto>>> Handle(
        GetHomeBannersQuery request,
        CancellationToken cancellationToken)
    {
        var banners = await discoveryQueries.GetHomeBannersAsync(
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<IReadOnlyList<HomeBannerDto>>.Success(banners);
    }
}