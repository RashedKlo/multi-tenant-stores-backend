using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HomeBannerRepository : IHomeBannerRepository
{
    private readonly AppDbContext _context;
    public HomeBannerRepository(AppDbContext context) => _context = context;

    public Task<HomeBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.HomeBanners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<HomeBanner>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        _context.HomeBanners
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);

    public void Add(HomeBanner banner) => _context.HomeBanners.Add(banner);
    public void Update(HomeBanner banner) => _context.HomeBanners.Update(banner);
    public void Delete(HomeBanner banner) => _context.HomeBanners.Remove(banner);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
