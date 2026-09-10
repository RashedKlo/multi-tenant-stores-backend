
using Domain.Aggregates.Cart;
namespace Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cart?> GetByCustomerAndStoreAsync(Guid customerId, Guid storeId, CancellationToken ct = default);
    Task<Cart?> GetByGuestAndStoreAsync(Guid guestSessionId, Guid storeId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}