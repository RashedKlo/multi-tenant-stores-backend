using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductOptionGroupRepository
{
    Task<ProductOptionGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductOptionGroup>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    void Add(ProductOptionGroup group);
    void Update(ProductOptionGroup group);
    void Delete(ProductOptionGroup group);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
