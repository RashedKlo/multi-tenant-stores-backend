using Domain.Entities;

namespace Domain.Interfaces;

public interface IModuleBannerRepository
{
    Task<ModuleBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ModuleBanner>> GetByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);

    void Add(ModuleBanner banner);
    void Update(ModuleBanner banner);
    void Delete(ModuleBanner banner);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
