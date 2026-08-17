using FluentValidation;

namespace Application.Catalog.Queries.GetStoreBanners;

public class GetStoreBannersValidator : AbstractValidator<GetStoreBannersQuery>
{
    public GetStoreBannersValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
