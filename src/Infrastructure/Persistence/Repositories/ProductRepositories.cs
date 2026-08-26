using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context) => _context = context;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    // The "precomputed JSON" read: images + option groups + their options,
    // all in one round trip. Three sibling collections via Include would
    // multiply rows (cartesian explosion) in a single SQL query, so this
    // uses AsSplitQuery — EF issues one SQL statement per collection instead
    // and stitches them together in memory. Always AsNoTracking: this is a
    // read-only projection for the API response, never mutated afterwards.
    public Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.OptionGroups.Where(g => g.IsActive && g.DeletedAt == null).OrderBy(g => g.DisplayOrder))
                .ThenInclude(g => g.Options.Where(o => o.IsActive && o.DeletedAt == null).OrderBy(o => o.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && p.DeletedAt == null, cancellationToken);

    public Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Products.AsNoTracking()
            .AnyAsync(p => p.Id == id && p.IsActive && p.DeletedAt == null, cancellationToken);

    // Used by cart/checkout validation to re-check several products at once
    // in a single round trip instead of one query per line item.
    public Task<List<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    // GET /api/sections/{id}/products?status=&minPrice=&maxPrice=&page=
    public async Task<(List<Product> Items, int TotalCount)> GetPagedBySectionAsync(
        Guid sectionId, bool? inStockOnly, decimal? minPrice, decimal? maxPrice,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Matches idx_products_active_by_section (section_id) WHERE is_active
        // AND deleted_at IS NULL — keep this predicate shape so Postgres can
        // use that partial index instead of scanning the whole table.
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.SectionId == sectionId && p.IsActive && p.DeletedAt == null);

        if (inStockOnly == true)
            query = query.Where(p => !p.TrackInventory || p.StockQuantity > 0);

        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.NameEn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Product product) => _context.Products.Add(product);
    public void Update(Product product) => _context.Products.Update(product);
    public void Delete(Product product) => _context.Products.Remove(product);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class ProductImageRepository : IProductImageRepository
{
    private readonly AppDbContext _context;
    public ProductImageRepository(AppDbContext context) => _context = context;

    public Task<List<ProductImage>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.ProductImages.AsNoTracking().ToListAsync(cancellationToken);

    public Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ProductImages.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AddAsync(ProductImage image, CancellationToken cancellationToken = default) =>
        await _context.ProductImages.AddAsync(image, cancellationToken);

    public void Update(ProductImage image) => _context.ProductImages.Update(image);
    public void Delete(ProductImage image) => _context.ProductImages.Remove(image);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class ProductOptionGroupRepository : IProductOptionGroupRepository
{
    private readonly AppDbContext _context;
    public ProductOptionGroupRepository(AppDbContext context) => _context = context;

    public Task<ProductOptionGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ProductOptionGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<List<ProductOptionGroup>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _context.ProductOptionGroups
            .AsNoTracking()
            .Include(g => g.Options.Where(o => o.IsActive && o.DeletedAt == null))
            .Where(g => g.ProductId == productId && g.IsActive && g.DeletedAt == null)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(cancellationToken);

    public void Add(ProductOptionGroup group) => _context.ProductOptionGroups.Add(group);
    public void Update(ProductOptionGroup group) => _context.ProductOptionGroups.Update(group);
    public void Delete(ProductOptionGroup group) => _context.ProductOptionGroups.Remove(group);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class ProductOptionRepository : IProductOptionRepository
{
    private readonly AppDbContext _context;
    public ProductOptionRepository(AppDbContext context) => _context = context;

    public Task<ProductOption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ProductOptions.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<List<ProductOption>> GetByOptionGroupIdAsync(Guid optionGroupId, CancellationToken cancellationToken = default) =>
        _context.ProductOptions
            .AsNoTracking()
            .Where(o => o.OptionGroupId == optionGroupId && o.IsActive && o.DeletedAt == null)
            .OrderBy(o => o.DisplayOrder)
            .ToListAsync(cancellationToken);

    // Powers the cart "option belongs to this product" check that used to be
    // a DB trigger: caller compares each option's OptionGroup.ProductId
    // against the cart item's ProductId. One query for the whole selection
    // set instead of one round trip per option id.
    public Task<List<ProductOption>> GetByIdsWithGroupAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        _context.ProductOptions
            .AsNoTracking()
            .Include(o => o.OptionGroup)
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken);

    public void Add(ProductOption option) => _context.ProductOptions.Add(option);
    public void Update(ProductOption option) => _context.ProductOptions.Update(option);
    public void Delete(ProductOption option) => _context.ProductOptions.Remove(option);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
