using Domain.Entities;

namespace Domain.Interfaces;

public interface IDiscountRepository
{
    Task<Discount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Discount>> GetActiveByStoreIdAsync(Guid storeId, DateTimeOffset asOf, CancellationToken cancellationToken = default);

    void Add(Discount discount);
    void Update(Discount discount);
    void Delete(Discount discount);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
