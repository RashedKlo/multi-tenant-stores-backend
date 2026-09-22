using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetHomeBanners;

public record GetHomeBannersQuery : IRequest<Result<IReadOnlyList<HomeBannerDto>>>, ICacheableQuery
{
    public string CacheKey => "banners:home";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}