using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FavoriteStoreRepository : IFavoriteStoreRepository
{
    private readonly AppDbContext _context;

    public FavoriteStoreRepository(AppDbContext context) => _context = context;

    public Task<bool> ExistsAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default) =>
        _context.FavoriteStores
            .AsNoTracking()
            .AnyAsync(f => f.CustomerId == customerId && f.StoreId == storeId, cancellationToken);

    public async Task<(List<FavoriteStore> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FavoriteStores
            .AsNoTracking()
            .Include(f => f.Store)
            .Where(f => f.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(FavoriteStore favorite, CancellationToken cancellationToken = default) =>
        await _context.FavoriteStores.AddAsync(favorite, cancellationToken);

    public async Task RemoveAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.FavoriteStores
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.StoreId == storeId, cancellationToken);

        if (favorite is not null)
            _context.FavoriteStores.Remove(favorite);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
