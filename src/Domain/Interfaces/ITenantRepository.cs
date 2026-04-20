using Domain.Entities;

namespace Domain.Interfaces;

public interface ITenantRepository
{
    Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    void Update(Tenant tenant);
    void Delete(Tenant tenant);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}