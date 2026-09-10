using FluentValidation;

namespace Application.Features.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithErrorCode("CartItemId.Required")
            .WithMessage("CartItemId must be a valid GUID.");

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
