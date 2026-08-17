using Domain.Entities;

namespace Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Both eager-load Items + ItemOptions — the shapes GET /api/cart needs.
    Task<Cart?> GetByCustomerAndStoreAsync(Guid customerId, Guid storeId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByGuestSessionAndStoreAsync(Guid guestSessionId, Guid storeId, CancellationToken cancellationToken = default);

    void Add(Cart cart);
    void Delete(Cart cart); // used for DELETE /api/cart and for the guest cart after login handoff

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICartItemRepository
{
    Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    // Implements the "same product + same options = same line" rule from the
    // add-to-cart flow: returns the existing line whose selected option ids
    // exactly match, or null if this is a genuinely new line.
    Task<CartItem?> FindMatchingLineAsync(
        Guid cartId,
        Guid productId,
        IReadOnlyCollection<Guid> optionIds,
        CancellationToken cancellationToken = default);

    void Add(CartItem item);
    void Update(CartItem item);
    void Delete(CartItem item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICartItemOptionRepository
{
    Task<List<CartItemOption>> GetByCartItemIdAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<CartItemOption> options, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<CartItemOption> options); // e.g. clearing selections before re-adding on update

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
