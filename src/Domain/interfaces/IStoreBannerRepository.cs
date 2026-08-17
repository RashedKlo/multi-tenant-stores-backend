using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreBannerRepository
{
    Task<List<StoreBanner>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StoreBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<StoreBanner>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task AddAsync(StoreBanner banner, CancellationToken cancellationToken = default);
    void Update(StoreBanner banner);
    void Delete(StoreBanner banner);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
