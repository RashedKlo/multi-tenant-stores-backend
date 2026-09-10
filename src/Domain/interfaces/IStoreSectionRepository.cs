using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreSectionRepository
{
    Task<StoreSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Backs GET /api/stores/{id}/sections?page=&pageSize= ("load more")
    Task<(List<StoreSection> Items, int TotalCount)> GetPagedByStoreIdAsync(
        Guid storeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(StoreSection section);
    void Update(StoreSection section);
    void Delete(StoreSection section);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
