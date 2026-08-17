using FluentValidation;

namespace Application.Catalog.Queries.GetStoreById;

public class GetStoreByIdValidator : AbstractValidator<GetStoreByIdQuery>
{
    public GetStoreByIdValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");
    }
}
