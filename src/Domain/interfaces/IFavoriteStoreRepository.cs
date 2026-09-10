using Domain.Entities;

namespace Domain.Interfaces;

public interface IFavoriteStoreRepository
{
    Task<bool> ExistsAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default);

    Task<(List<FavoriteStore> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(FavoriteStore favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
