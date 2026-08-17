using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Discovery.Queries.GetHomeBanners;

public class GetHomeBannersHandler(IHomeBannerRepository repository, ICacheService cache)
    : IRequestHandler<GetHomeBannersQuery, Result<List<HomeBannerDto>>>
{
    private const string CacheKey = "home:banners";

    public async Task<Result<List<HomeBannerDto>>> Handle(
        GetHomeBannersQuery request, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<List<HomeBannerDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return Result<List<HomeBannerDto>>.Success(cached);

        var banners = await repository.GetActiveOrderedAsync(cancellationToken);
        var dtos = banners.Select(HomeBannerDto.FromEntity).ToList();

        // Highest traffic-per-byte endpoint — longest TTL in this module.
        await cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(30), cancellationToken);
        return Result<List<HomeBannerDto>>.Success(dtos);
    }
}