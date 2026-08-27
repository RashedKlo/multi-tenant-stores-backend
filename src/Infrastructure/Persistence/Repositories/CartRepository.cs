using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context) => _context = context;

    public Task<Cart?> GetForUpdateByCustomerAndStoreAsync(
        Guid customerId, Guid storeId, CancellationToken ct = default)
        => GetForUpdateAsync(c => c.CustomerId == customerId && c.StoreId == storeId, ct);

    public Task<Cart?> GetForUpdateByGuestSessionAndStoreAsync(
        Guid guestSessionId, Guid storeId, CancellationToken ct = default)
        => GetForUpdateAsync(c => c.GuestSessionId == guestSessionId && c.StoreId == storeId, ct);

    private Task<Cart?> GetForUpdateAsync(
        System.Linq.Expressions.Expression<Func<Cart, bool>> predicate, CancellationToken ct)
    {
        return _context.Carts
    .Include(c => c.CartItems)
        .ThenInclude(ci => ci.CartItemOptions)
    .AsSplitQuery()
    .AsTracking()   // explicit; default is tracking, but make intent clear
    .FirstOrDefaultAsync(predicate, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
        => await _context.Carts.AddAsync(cart, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}