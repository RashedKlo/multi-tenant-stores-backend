using Application.Common.Interfaces;
using Application.Stores.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Stores.Commands.CreateStore;

public class CreateStoreHandler(IStoreRepository storeRepository, ICacheService cache) : IRequestHandler<CreateStoreCommand, StoreDto>
{

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = Store.Create(request.TenantId, request.Name, request.Description, request.LogoUrl, request.BannerUrl);

        await storeRepository.AddAsync(store, cancellationToken);
        await storeRepository.SaveChangesAsync(cancellationToken);

        // Invalidate list cache since data changed
        await cache.RemoveAsync("stores:all", cancellationToken);

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
