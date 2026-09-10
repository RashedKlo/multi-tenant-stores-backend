using Domain.Entities;

namespace Domain.Interfaces;

public interface IDiscountSectionRepository
{
    Task<List<DiscountSection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiscountSection?> GetByIdsAsync(Guid discountId, Guid sectionId, CancellationToken cancellationToken = default);

    Task AddAsync(DiscountSection ds, CancellationToken cancellationToken = default);
    void Delete(DiscountSection ds);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
