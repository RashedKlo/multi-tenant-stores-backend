using Application.Common.Interfaces;
using Application.Stores.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Stores.Commands.UpdateStore;

public class UpdateStoreHandler(IStoreRepository repository, ICacheService cache) : IRequestHandler<UpdateStoreCommand, StoreDto?>
{

    public async Task<StoreDto?> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (store is null) return null;

        store.Update(
               request.Name,
               request.Description,
               request.LogoUrl,
               request.BannerUrl,
               request.IsActive
           );

        repository.Update(store);
        await repository.SaveChangesAsync(cancellationToken);

        // Invalidate both caches
        await cache.RemoveAsync("stores:all", cancellationToken);
        await cache.RemoveAsync($"stores:{store.Id}", cancellationToken);

        return new StoreDto(
            store.Id,
            store.TenantId,
            store.Name,
            store.Description,
            store.LogoUrl,
            store.BannerUrl,
            store.IsActive,
            store.CreatedAt,
            store.UpdatedAt
        );
    }
}
