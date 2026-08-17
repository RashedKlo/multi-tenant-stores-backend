using FluentValidation;

namespace Application.Favorites.Commands.RemoveFavoriteProduct;

public class RemoveFavoriteProductValidator : AbstractValidator<RemoveFavoriteProductCommand>
{
    public RemoveFavoriteProductValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithErrorCode("ProductId.Required")
            .WithMessage("ProductId must be a valid GUID.");
    }
}
