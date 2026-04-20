using Application.Common.Interfaces;
using Application.Stores.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Stores.Queries.GetAllStores;

public class GetAllStoresHandler : IRequestHandler<GetAllStoresQuery, List<StoreDto>>
{
    private readonly IStoreRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "stores:all";

    public GetAllStoresHandler(IStoreRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetAsync<List<StoreDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var stores = await _repository.GetAllAsync(cancellationToken);

        var dtos = stores.Select(s => new StoreDto(
            s.Id, s.TenantId, s.Name, s.Description, s.LogoUrl, s.BannerUrl, s.IsActive, s.CreatedAt, s.UpdatedAt
        )).ToList();

        // Store in cache for 10 minutes
        await _cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}
