using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Delete(Product product);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
