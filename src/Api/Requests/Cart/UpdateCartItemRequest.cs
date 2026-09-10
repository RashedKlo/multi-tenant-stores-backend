// Api/Requests/Cart/UpdateCartItemRequest.cs
namespace Api.Requests.Cart;

public sealed record UpdateCartItemRequest(
    Guid StoreId,
    int Quantity);
