using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductImageRepository
{
    Task<List<ProductImage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ProductImage image, CancellationToken cancellationToken = default);
    void Update(ProductImage image);
    void Delete(ProductImage image);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
