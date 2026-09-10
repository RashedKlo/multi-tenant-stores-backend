// Infrastructure/Persistence/Repositories/CartRepository.cs
using Domain.Aggregates.Cart;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _db;

    public CartRepository(AppDbContext db) => _db = db;

    public Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => QueryForUpdate()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cart?> GetByCustomerAndStoreAsync(
        Guid customerId,
        Guid storeId,
        CancellationToken ct = default)
        => QueryForUpdate()
            .FirstOrDefaultAsync(
                c => c.CustomerId == customerId && c.StoreId == storeId,
                ct);

    public Task<Cart?> GetByGuestAndStoreAsync(
        Guid guestSessionId,
        Guid storeId,
        CancellationToken ct = default)
        => QueryForUpdate()
            .FirstOrDefaultAsync(
                c => c.GuestSessionId == guestSessionId && c.StoreId == storeId,
                ct);

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
        => await _db.Set<Cart>().AddAsync(cart, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    // ---------- private ----------

    /// <summary>
    /// Loads the full aggregate (Cart + Items + Options) with change tracking.
    /// AsSplitQuery avoids cartesian product.
    /// </summary>
    private IQueryable<Cart> QueryForUpdate()
        => _db.Set<Cart>()
            .Include(c => c.Items)
                .ThenInclude(i => i.Options)
            .AsSplitQuery()
            .AsTracking();
}