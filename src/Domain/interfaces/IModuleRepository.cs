using Domain.Entities;

namespace Domain.Interfaces;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Module?> GetReadByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Module>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    void Add(Module module);
    void Update(Module module);
    void Delete(Module module);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
