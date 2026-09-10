using FluentValidation;

namespace Application.Features.Cart.Commands.ClearCart;

public sealed class ClearCartValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
