using Domain.Entities;

namespace Domain.Interfaces;

public interface IFavoriteProductRepository
{
    Task<bool> ExistsAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);
    Task<FavoriteProduct?> GetAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);

    Task<(List<FavoriteProduct> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(FavoriteProduct favorite, CancellationToken cancellationToken = default);
    void Delete(FavoriteProduct favorite);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IFavoriteStoreRepository
{
    Task<bool> ExistsAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default);
    Task<FavoriteStore?> GetAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default);

    Task<(List<FavoriteStore> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(FavoriteStore favorite, CancellationToken cancellationToken = default);
    void Delete(FavoriteStore favorite);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
