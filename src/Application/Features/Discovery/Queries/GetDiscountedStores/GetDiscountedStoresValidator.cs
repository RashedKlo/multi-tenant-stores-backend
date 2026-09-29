using FluentValidation;

namespace Application.Discovery.Queries.GetDiscountedStores;

public class GetDiscountedStoresValidator : AbstractValidator<GetDiscountedStoresQuery>
{
    public GetDiscountedStoresValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
            .WithErrorCode("PageNumber.Invalid").WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize).InclusiveBetween(1, 50)
            .WithErrorCode("PageSize.Invalid").WithMessage("PageSize must be between 1 and 50.");
    }
}