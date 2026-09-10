// Application/Common/Interfaces/ICartQueries.cs
using Application.Features.Cart.DTOs;
namespace Application.Common.Interfaces;

public interface ICartQueries
{
    Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(
        Guid? customerId, Guid? guestSessionId, Language lang);

    Task<CheckoutCartDto?> GetCartForCheckoutAsync(
        Guid customerId, Guid storeId,
        CancellationToken cancellationToken = default);
}