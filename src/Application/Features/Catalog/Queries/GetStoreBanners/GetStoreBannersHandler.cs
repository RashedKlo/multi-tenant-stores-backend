using Application.Catalog.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Catalog.Queries.GetStoreBanners;

public class GetStoreBannersHandler(IStoreBannerRepository repository)
    : IRequestHandler<GetStoreBannersQuery, Result<List<StoreBannerDto>>>
{
    public async Task<Result<List<StoreBannerDto>>> Handle(
        GetStoreBannersQuery request, CancellationToken cancellationToken)
    {
        var banners = await repository.GetByStoreIdAsync(request.StoreId, cancellationToken);
        var dtos = banners.Select(StoreBannerDto.FromEntity).ToList();
        return Result<List<StoreBannerDto>>.Success(dtos);
    }
}
