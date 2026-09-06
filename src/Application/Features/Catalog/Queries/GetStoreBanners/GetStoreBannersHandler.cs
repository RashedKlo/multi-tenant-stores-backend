using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Catalog.Queries.GetStoreBanners;

public class GetStoreBannersHandler(IStoreBannerRepository repository, ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoreBannersQuery, Result<List<StoreBannerDto>>>
{
    public async Task<Result<List<StoreBannerDto>>> Handle(
        GetStoreBannersQuery request, CancellationToken cancellationToken)
    {
        var banners = await repository.GetByStoreIdAsync(request.StoreId, cancellationToken);
        var dtos = banners.Select(b => StoreBannerDto.FromEntity(b, currentLanguageProvider.Language)).ToList();
        return Result<List<StoreBannerDto>>.Success(dtos);
    }
}
