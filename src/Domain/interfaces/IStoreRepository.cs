using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreRepository
{
    // Tracked — use for command flows (create/update a store).
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Untracked, with images/sections eager-loaded as needed by the caller —
    // use for the customer-facing "store detail" read.
    Task<Store?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    // Backs GET /api/modules/{id}/stores?categoryId=&search=
    Task<(List<Store> Items, int TotalCount)> GetPagedByModuleAsync(
        Guid moduleId,
        Guid? categoryId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Store store);
    void Update(Store store);
    void Delete(Store store);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
