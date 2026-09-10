using Domain.Entities;

namespace Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(Tenant tenant);
    void Update(Tenant tenant);
    void Delete(Tenant tenant);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
