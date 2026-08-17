using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;
    public TenantRepository(AppDbContext context) => _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().AnyAsync(t => t.Email == email, cancellationToken);

    public void Add(Tenant tenant) => _context.Tenants.Add(tenant);
    public void Update(Tenant tenant) => _context.Tenants.Update(tenant);
    public void Delete(Tenant tenant) => _context.Tenants.Remove(tenant);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _context;
    public ModuleRepository(AppDbContext context) => _context = context;

    public Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Modules.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    // Public storefront read — no tracking needed, ordered exactly how the
    // home page renders the module list. Backed by is_active default index.
    public Task<List<Module>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        _context.Modules
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

    public void Add(Module module) => _context.Modules.Add(module);
    public void Update(Module module) => _context.Modules.Update(module);
    public void Delete(Module module) => _context.Modules.Remove(module);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

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

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) => _context = context;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Category?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Category>> GetByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default) =>
        _context.Categories
            .AsNoTracking()
            .Where(c => c.ModuleId == moduleId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<(List<Category> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, Guid? moduleId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.AsNoTracking().AsQueryable();
        if (moduleId.HasValue) query = query.Where(c => c.ModuleId == moduleId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.DisplayOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Category category) => _context.Categories.Add(category);
    public void Update(Category category) => _context.Categories.Update(category);
    public void Delete(Category category) => _context.Categories.Remove(category);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

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

public class StoreCategoryRepository : IStoreCategoryRepository
{
    private readonly AppDbContext _context;
    public StoreCategoryRepository(AppDbContext context) => _context = context;

    public Task<List<StoreCategory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.StoreCategories.AsNoTracking().ToListAsync(cancellationToken);

    public Task<StoreCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreCategories.FirstOrDefaultAsync(sc => sc.CategoryId == id, cancellationToken);

    public Task<List<StoreCategory>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default) =>
        _context.StoreCategories.AsNoTracking().Where(sc => sc.StoreId == storeId).ToListAsync(cancellationToken);

    public Task<List<StoreCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        _context.StoreCategories.AsNoTracking().Where(sc => sc.CategoryId == categoryId).ToListAsync(cancellationToken);

    public async Task AddAsync(StoreCategory sc, CancellationToken cancellationToken = default) =>
        await _context.StoreCategories.AddAsync(sc, cancellationToken);

    public void Delete(StoreCategory sc) => _context.StoreCategories.Remove(sc);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

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

public class StoreSectionRepository : IStoreSectionRepository
{
    private readonly AppDbContext _context;
    public StoreSectionRepository(AppDbContext context) => _context = context;

    public Task<StoreSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreSections.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    // "Load more" pagination for the store detail page.
    public async Task<(List<StoreSection> Items, int TotalCount)> GetPagedByStoreIdAsync(
        Guid storeId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.StoreSections
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.DisplayOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(StoreSection section) => _context.StoreSections.Add(section);
    public void Update(StoreSection section) => _context.StoreSections.Update(section);
    public void Delete(StoreSection section) => _context.StoreSections.Remove(section);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
