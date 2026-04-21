using Domain.Entities;

namespace Domain.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Department>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task AddAsync(Department department, CancellationToken cancellationToken = default);
    void Update(Department department);
    void Delete(Department department);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
