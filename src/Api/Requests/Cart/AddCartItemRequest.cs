// Api/Requests/Cart/AddCartItemRequest.cs
namespace Api.Requests.Cart;

public sealed record AddCartItemRequest(
    Guid StoreId,
    Guid ProductId,
    Guid DiscountId,
    int Quantity,
    string? Notes,
    List<Guid>? OptionIds);