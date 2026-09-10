using FluentValidation;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public sealed class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithErrorCode("CartItemId.Required")
            .WithMessage("CartItemId must be a valid GUID.");

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithErrorCode("Quantity.NotPositive")
            .WithMessage("Quantity must be greater than zero.");
    }
}
