using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreRepository
{
    Task<List<Store>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Store Store, CancellationToken cancellationToken = default);
    void Update(Store Store);
    void Delete(Store Store);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}