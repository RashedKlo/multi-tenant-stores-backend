using Domain.Entities;

namespace Domain.Interfaces;

public interface IFavoriteProductRepository
{
    Task<bool> ExistsAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);

    Task<(List<FavoriteProduct> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(FavoriteProduct favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
