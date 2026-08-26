using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly AppDbContext _context;
    public StoreRepository(AppDbContext context) => _context = context;

    public Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Stores.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    // Store detail page: banners are a small, always-needed collection here,
    // so one Include is fine — no cartesian blow-up with a single collection.
    public Task<Store?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Stores
            .AsNoTracking()
            .Include(s => s.StoreBanners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<List<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Stores.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);

    // GET /api/modules/{id}/stores?categoryId=&search=
    // categoryId filters via the StoreCategories junction; search hits the
    // trigram GIN index on name_en/name_ar through ILIKE, not a leading-wildcard
    // scan of the whole table.
    public async Task<(List<Store> Items, int TotalCount)> GetPagedByModuleAsync(
        Guid moduleId, Guid? categoryId, string? search,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Stores
            .AsNoTracking()
            .Where(s => s.ModuleId == moduleId && s.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.StoreCategories.Any(sc => sc.CategoryId == categoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.NameEn, $"%{search}%") ||
                EF.Functions.ILike(s.NameAr, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.Rating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Store store) => _context.Stores.Add(store);
    public void Update(Store store) => _context.Stores.Update(store);
    public void Delete(Store store) => _context.Stores.Remove(store);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
