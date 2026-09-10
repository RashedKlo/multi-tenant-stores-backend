using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FavoriteProductRepository : IFavoriteProductRepository
{
    private readonly AppDbContext _context;

    public FavoriteProductRepository(AppDbContext context) => _context = context;

    // Backs the isFavorite flag on product list/detail responses —
    // a single indexed existence check, not a full row fetch.
    public Task<bool> ExistsAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default) =>
        _context.FavoriteProducts
            .AsNoTracking()
            .AnyAsync(f => f.CustomerId == customerId && f.ProductId == productId, cancellationToken);

    public async Task<(List<FavoriteProduct> Items, int TotalCount)> GetPagedByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FavoriteProducts
            .AsNoTracking()
            .Include(f => f.Product)
            .Include(f => f.Product!.Images)
            .Where(f => f.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(FavoriteProduct favorite, CancellationToken cancellationToken = default) =>
        await _context.FavoriteProducts.AddAsync(favorite, cancellationToken);

    public async Task RemoveAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.FavoriteProducts
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.ProductId == productId, cancellationToken);

        if (favorite is not null)
            _context.FavoriteProducts.Remove(favorite);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
