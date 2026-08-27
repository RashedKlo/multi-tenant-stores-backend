using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ICartQueries
{
    /// <summary>
    /// Returns cart items for the given customer or guest session.
    /// Exactly one of the two should be non-null; pass the other as null.
    /// </summary>
    Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(Guid? customerId, Guid? guestSessionId);
}