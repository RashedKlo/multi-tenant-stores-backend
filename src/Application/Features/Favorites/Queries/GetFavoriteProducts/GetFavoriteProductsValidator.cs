using FluentValidation;

namespace Application.Favorites.Queries.GetFavoriteProducts;

public class GetFavoriteProductsValidator : AbstractValidator<GetFavoriteProductsQuery>
{
    public GetFavoriteProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PageNumber.Invalid")
            .WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PageSize.Invalid")
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
