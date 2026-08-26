using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ModuleBannerRepository : IModuleBannerRepository
{
    private readonly AppDbContext _context;
    public ModuleBannerRepository(AppDbContext context) => _context = context;

    public Task<ModuleBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ModuleBanners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<ModuleBanner>> GetByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default) =>
        _context.ModuleBanners
            .AsNoTracking()
            .Where(b => b.ModuleId == moduleId && b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);

    public void Add(ModuleBanner banner) => _context.ModuleBanners.Add(banner);
    public void Update(ModuleBanner banner) => _context.ModuleBanners.Update(banner);
    public void Delete(ModuleBanner banner) => _context.ModuleBanners.Remove(banner);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
