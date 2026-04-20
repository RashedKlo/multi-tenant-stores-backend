using Application.Common.Interfaces;
using Application.Stores.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Stores.Queries.GetStoreById;

public class GetStoreByIdHandler : IRequestHandler<GetStoreByIdQuery, StoreDto?>
{
    private readonly IStoreRepository _repository;
    private readonly ICacheService _cache;

    public GetStoreByIdHandler(IStoreRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"stores:{request.Id}";

        var cached = await _cache.GetAsync<StoreDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var store = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (store is null) return null;

        var dto = new StoreDto(
            store.Id, store.TenantId, store.Name, store.Description, store.LogoUrl, store.BannerUrl, store.IsActive, store.CreatedAt, store.UpdatedAt
        );

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);

        return dto;
    }
}
