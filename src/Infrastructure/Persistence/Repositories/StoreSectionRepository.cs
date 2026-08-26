using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

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
