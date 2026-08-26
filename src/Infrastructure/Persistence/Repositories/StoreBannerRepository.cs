using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StoreBannerRepository : IStoreBannerRepository
{
    private readonly AppDbContext _context;
    public StoreBannerRepository(AppDbContext context) => _context = context;

    public Task<List<StoreBanner>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.StoreBanners.AsNoTracking().ToListAsync(cancellationToken);

    public Task<StoreBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreBanners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<StoreBanner>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default) =>
        _context.StoreBanners
            .AsNoTracking()
            .Where(b => b.StoreId == storeId && b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(StoreBanner banner, CancellationToken cancellationToken = default) =>
        await _context.StoreBanners.AddAsync(banner, cancellationToken);

    public void Update(StoreBanner banner) => _context.StoreBanners.Update(banner);
    public void Delete(StoreBanner banner) => _context.StoreBanners.Remove(banner);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
