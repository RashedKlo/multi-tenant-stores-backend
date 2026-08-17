using FluentValidation;

namespace Application.Discovery.Queries.GetStoresByModule;

public class GetStoresByModuleValidator : AbstractValidator<GetStoresByModuleQuery>
{
    public GetStoresByModuleValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithErrorCode("ModuleId.Required")
            .WithMessage("ModuleId must be a valid GUID.");

        RuleFor(x => x.CategoryId)
            .Must(id => id == null || id != Guid.Empty)
            .WithErrorCode("CategoryId.Invalid")
            .WithMessage("CategoryId must be a valid GUID when provided.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode("Search.TooLong")
            .WithMessage("Search term must not exceed 100 characters.");

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