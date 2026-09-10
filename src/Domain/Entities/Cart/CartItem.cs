// Domain/Aggregates/Cart/CartItem.cs
using Domain.Common;

namespace Domain.Aggregates.Cart;

public sealed class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<CartItemOption> _options = new();
    public IReadOnlyCollection<CartItemOption> Options => _options.AsReadOnly();

    private CartItem() { }

    internal static Result<CartItem> Create(
        Guid cartId,
        Guid productId,
        int quantity,
        string? notes = null,
        IEnumerable<Guid>? optionIds = null)
    {
        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = item
            .SetCartId(cartId)
            .Bind(() => item.SetProductId(productId))
            .Bind(() => item.SetQuantity(quantity))
            .Bind(() => item.SetNotes(notes));

        if (result.IsFailure)
            return Result<CartItem>.Failure(result.Errors);

        if (optionIds is not null)
        {
            foreach (var optionId in optionIds.Distinct())
            {
                var optResult = CartItemOption.Create(item.Id, optionId);
                if (optResult.IsFailure)
                    return Result<CartItem>.Failure(optResult.Errors);

                item._options.Add(optResult.Value!);
            }
        }

        return Result<CartItem>.Success(item);
    }

    // ---------- Field-level setters (invariants live here) ----------

    private Result SetCartId(Guid cartId)
    {
        if (cartId == Guid.Empty)
            return Result.Failure(Error.Validation("CartItem.CartId.Required", "CartId is required."));

        CartId = cartId;
        return Result.Success();
    }

    private Result SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            return Result.Failure(Error.Validation("CartItem.ProductId.Required", "ProductId is required."));

        ProductId = productId;
        return Result.Success();
    }

    public Result SetQuantity(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation("CartItem.Quantity.NotPositive", "Quantity must be greater than zero."));

        Quantity = quantity;
        Touch();
        return Result.Success();
    }

    public Result SetNotes(string? notes)
    {
        if (notes is null)
        {
            Notes = null;
            Touch();
            return Result.Success();
        }

        var trimmed = notes.Trim();
        if (trimmed.Length == 0)
        {
            Notes = null;
            Touch();
            return Result.Success();
        }

        if (trimmed.Length > 500)
            return Result.Failure(Error.Validation("CartItem.Notes.TooLong", "Notes cannot exceed 500 characters."));

        Notes = trimmed;
        Touch();
        return Result.Success();
    }

    public Result IncreaseQuantity(int amount = 1)
    {
        if (amount <= 0)
            return Result.Failure(Error.Validation("CartItem.Amount.NotPositive", "Amount must be greater than zero."));

        Quantity += amount;
        Touch();
        return Result.Success();
    }

    internal void Touch() => UpdatedAt = DateTime.UtcNow;

    internal bool HasSameOptions(IEnumerable<Guid>? optionIds)
    {
        var existing = _options.Select(o => o.OptionId).OrderBy(x => x);
        var incoming = (optionIds ?? Enumerable.Empty<Guid>()).OrderBy(x => x);
        return existing.SequenceEqual(incoming);
    }
}