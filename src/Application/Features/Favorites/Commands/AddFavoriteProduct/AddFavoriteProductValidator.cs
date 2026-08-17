using FluentValidation;

namespace Application.Favorites.Commands.AddFavoriteProduct;

public class AddFavoriteProductValidator : AbstractValidator<AddFavoriteProductCommand>
{
    public AddFavoriteProductValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithErrorCode("ProductId.Required")
            .WithMessage("ProductId must be a valid GUID.");
    }
}
