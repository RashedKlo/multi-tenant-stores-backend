// Domain/Aggregates/Cart/Cart.cs
using Domain.Common;

namespace Domain.Aggregates.Cart;

public sealed class Cart
{
    public Guid Id { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? GuestSessionId { get; private set; }
    public Guid StoreId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    // ---------- Factories ----------

    public static Result<Cart> Create(Guid storeId, Guid? customerId = null, Guid? guestSessionId = null)
    {
        if (customerId is null && guestSessionId is null)
            return Result<Cart>.Failure(
                Error.Validation("Cart.Owner.Required", "Either CustomerId or GuestSessionId is required."));

        if (customerId is not null && guestSessionId is not null)
            return Result<Cart>.Failure(
                Error.Validation("Cart.Owner.Conflict", "Only one owner identifier can be supplied."));

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return cart
            .SetStoreId(storeId)
            .Bind(() => customerId is not null
                ? cart.SetCustomerId(customerId.Value)
                : cart.SetGuestSessionId(guestSessionId!.Value))
            .Bind(() => Result<Cart>.Success(cart));
    }

    // ---------- Field-level setters ----------

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("Cart.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        GuestSessionId = null;
        return Result.Success();
    }

    private Result SetGuestSessionId(Guid guestSessionId)
    {
        if (guestSessionId == Guid.Empty)
            return Result.Failure(Error.Validation("Cart.GuestSessionId.Required", "GuestSessionId is required."));

        GuestSessionId = guestSessionId;
        CustomerId = null;
        return Result.Success();
    }

    private Result SetStoreId(Guid storeId)
    {
        if (storeId == Guid.Empty)
            return Result.Failure(Error.Validation("Cart.StoreId.Required", "StoreId is required."));

        StoreId = storeId;
        return Result.Success();
    }

    // ---------- Aggregate behavior ----------

    public Result AddItem(
        Guid productId,
        int quantity,
        string? notes = null,
        IEnumerable<Guid>? optionIds = null)
    {
        var existing = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.HasSameOptions(optionIds));

        if (existing is not null)
        {
            var increase = existing.IncreaseQuantity(quantity);
            if (increase.IsFailure) return increase;
            Touch();
            return Result.Success();
        }

        var itemResult = CartItem.Create(Id, productId, quantity, notes, optionIds);
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Errors);

        _items.Add(itemResult.Value!);
        Touch();
        return Result.Success();
    }

    public Result UpdateItemQuantity(Guid cartItemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("CartItem.NotFound", "Cart item not found."));

        var result = item.SetQuantity(quantity);
        if (result.IsFailure) return result;

        Touch();
        return Result.Success();
    }

    public Result UpdateItemNotes(Guid cartItemId, string? notes)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("CartItem.NotFound", "Cart item not found."));

        var result = item.SetNotes(notes);
        if (result.IsFailure) return result;

        Touch();
        return Result.Success();
    }

    public Result RemoveItem(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("CartItem.NotFound", "Cart item not found."));

        _items.Remove(item);
        Touch();
        return Result.Success();
    }

    public Result Clear()
    {
        _items.Clear();
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}