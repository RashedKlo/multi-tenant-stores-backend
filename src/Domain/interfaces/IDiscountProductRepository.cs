using Domain.Entities;

namespace Domain.Interfaces;


public interface IDiscountProductRepository
{
    Task<List<DiscountProduct>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiscountProduct?> GetByIdsAsync(Guid discountId, Guid ProductId, CancellationToken cancellationToken = default);

    Task AddAsync(DiscountProduct ds, CancellationToken cancellationToken = default);
    void Delete(DiscountProduct ds);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
