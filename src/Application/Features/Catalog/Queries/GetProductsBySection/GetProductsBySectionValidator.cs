using FluentValidation;

namespace Application.Catalog.Queries.GetProductsBySection;

public class GetProductsBySectionValidator : AbstractValidator<GetProductsBySectionQuery>
{
    public GetProductsBySectionValidator()
    {
        RuleFor(x => x.SectionId)
            .NotEmpty()
            .WithErrorCode("SectionId.Required")
            .WithMessage("SectionId must be a valid GUID.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithErrorCode("MinPrice.Invalid")
            .WithMessage("MinPrice must be greater than or equal to 0.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue)
            .WithErrorCode("MaxPrice.Invalid")
            .WithMessage("MaxPrice must be greater than or equal to 0.");

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithErrorCode("PriceRange.Invalid")
            .WithMessage("MinPrice must not be greater than MaxPrice.");

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
