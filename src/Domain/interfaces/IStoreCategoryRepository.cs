using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreCategoryRepository
{
    Task<List<StoreCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StoreCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<StoreCategory>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<List<StoreCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(StoreCategory sc, CancellationToken cancellationToken = default);
    void Delete(StoreCategory sc);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
