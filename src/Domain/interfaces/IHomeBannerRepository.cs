using Domain.Entities;

namespace Domain.Interfaces;

public interface IHomeBannerRepository
{
    Task<HomeBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<HomeBanner>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    void Add(HomeBanner banner);
    void Update(HomeBanner banner);
    void Delete(HomeBanner banner);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
