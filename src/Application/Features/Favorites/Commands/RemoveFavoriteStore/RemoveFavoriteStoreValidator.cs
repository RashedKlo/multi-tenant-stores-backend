using FluentValidation;

namespace Application.Favorites.Commands.RemoveFavoriteStore;

public class RemoveFavoriteStoreValidator : AbstractValidator<RemoveFavoriteStoreCommand>
{
    public RemoveFavoriteStoreValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
