using FluentValidation;

namespace Application.Catalog.Queries.GetProductById;

public class GetProductByIdValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithErrorCode("ProductId.Required")
            .WithMessage("ProductId must be a valid GUID.");
    }
}
