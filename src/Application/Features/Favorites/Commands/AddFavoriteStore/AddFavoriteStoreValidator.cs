using FluentValidation;

namespace Application.Favorites.Commands.AddFavoriteStore;

public class AddFavoriteStoreValidator : AbstractValidator<AddFavoriteStoreCommand>
{
    public AddFavoriteStoreValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
