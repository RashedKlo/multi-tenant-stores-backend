using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

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
