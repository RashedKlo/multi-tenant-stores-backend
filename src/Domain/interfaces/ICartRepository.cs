using Domain.Entities;

namespace Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetForUpdateByCustomerAndStoreAsync(Guid customerId, Guid storeId, CancellationToken ct = default);
    Task<Cart?> GetForUpdateByGuestSessionAndStoreAsync(Guid guestSessionId, Guid storeId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}