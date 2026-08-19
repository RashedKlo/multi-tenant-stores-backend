using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ICartQueries
{
    Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(Guid cartId);
}