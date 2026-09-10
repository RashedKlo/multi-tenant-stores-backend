// Domain/Aggregates/Cart/CartItemOption.cs
using Domain.Common;

namespace Domain.Aggregates.Cart;

public sealed class CartItemOption
{
    public Guid CartItemId { get; private set; }
    public Guid OptionId { get; private set; }

    private CartItemOption() { }

    internal static Result<CartItemOption> Create(Guid cartItemId, Guid optionId)
    {
        var option = new CartItemOption();

        var result = option
            .SetCartItemId(cartItemId)
            .Bind(() => option.SetOptionId(optionId));

        return result.IsFailure
            ? Result<CartItemOption>.Failure(result.Errors)
            : Result<CartItemOption>.Success(option);
    }

    private Result SetCartItemId(Guid cartItemId)
    {
        if (cartItemId == Guid.Empty)
            return Result.Failure(Error.Validation("CartItemOption.CartItemId.Required", "CartItemId is required."));

        CartItemId = cartItemId;
        return Result.Success();
    }

    private Result SetOptionId(Guid optionId)
    {
        if (optionId == Guid.Empty)
            return Result.Failure(Error.Validation("CartItemOption.OptionId.Required", "OptionId is required."));

        OptionId = optionId;
        return Result.Success();
    }
}