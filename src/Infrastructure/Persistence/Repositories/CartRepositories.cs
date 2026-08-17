using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;
    public CartRepository(AppDbContext context) => _context = context;

    public Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Carts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    // GET /api/cart?storeId= — tracked (not AsNoTracking), because the
    // typical caller right after reading is the add/update-item flow that
    // mutates this same cart in the same unit of work.
    public Task<Cart?> GetByCustomerAndStoreAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default) =>
        _context.Carts
            .AsSplitQuery()
            .Include(c => c.CartItems)
                .ThenInclude(i => i.CartItemOptions)
                    .ThenInclude(o => o.Option)
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.StoreId == storeId, cancellationToken);

    public Task<Cart?> GetByGuestSessionAndStoreAsync(Guid guestSessionId, Guid storeId, CancellationToken cancellationToken = default) =>
        _context.Carts
            .AsSplitQuery()
            .Include(c => c.CartItems)
                .ThenInclude(i => i.CartItemOptions)
                    .ThenInclude(o => o.Option)
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.GuestSessionId == guestSessionId && c.StoreId == storeId, cancellationToken);

    public void Add(Cart cart) => _context.Carts.Add(cart);
    public void Delete(Cart cart) => _context.Carts.Remove(cart); // cascades to Car.CartItems/CartItemOptions

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class CartItemRepository : ICartItemRepository
{
    private readonly AppDbContext _context;
    public CartItemRepository(AppDbContext context) => _context = context;

    public Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.CartItems
            .Include(i => i.CartItemOptions)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<List<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default) =>
        _context.CartItems
            .AsNoTracking()
            .Include(i => i.CartItemOptions)
            .Where(i => i.CartId == cartId)
            .ToListAsync(cancellationToken);

    // "Same product + same options = same line": narrow to same-product
    // candidates first (cheap, indexed on CartId+ProductId), then compare
    // each candidate's exact option-id set in memory. A cart realistically
    // has a handful of lines per product at most, so this beats building a
    // generated SQL set-equality expression for negligible gain.
    public async Task<CartItem?> FindMatchingLineAsync(
        Guid cartId, Guid productId, IReadOnlyCollection<Guid> optionIds, CancellationToken cancellationToken = default)
    {
        var candidates = await _context.CartItems
            .Include(i => i.CartItemOptions)
            .Where(i => i.CartId == cartId && i.ProductId == productId)
            .ToListAsync(cancellationToken);

        var selected = optionIds.ToHashSet();

        return candidates.FirstOrDefault(c =>
        {
            var existing = c.CartItemOptions.Select(o => o.OptionId).ToHashSet();
            return existing.SetEquals(selected);
        });
    }

    public void Add(CartItem item) => _context.CartItems.Add(item);
    public void Update(CartItem item) => _context.CartItems.Update(item);
    public void Delete(CartItem item) => _context.CartItems.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class CartItemOptionRepository : ICartItemOptionRepository
{
    private readonly AppDbContext _context;
    public CartItemOptionRepository(AppDbContext context) => _context = context;

    public Task<List<CartItemOption>> GetByCartItemIdAsync(Guid cartItemId, CancellationToken cancellationToken = default) =>
        _context.CartItemOptions
            .AsNoTracking()
            .Where(o => o.CartItemId == cartItemId)
            .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<CartItemOption> options, CancellationToken cancellationToken = default) =>
        await _context.CartItemOptions.AddRangeAsync(options, cancellationToken);

    public void DeleteRange(IEnumerable<CartItemOption> options) => _context.CartItemOptions.RemoveRange(options);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
