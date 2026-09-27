using FluentValidation;

namespace Application.Discovery.Queries.GetNearByStores;

public sealed class GetNearByStoresValidator : AbstractValidator<GetNearByStoresQuery>
{
    public GetNearByStoresValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m)
            .WithErrorCode("Latitude.Invalid")
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m)
            .WithErrorCode("Longitude.Invalid")
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.RadiusKm)
            .InclusiveBetween(1, 100)
            .WithErrorCode("RadiusKm.Invalid")
            .WithMessage("RadiusKm must be between 1 and 100.");

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