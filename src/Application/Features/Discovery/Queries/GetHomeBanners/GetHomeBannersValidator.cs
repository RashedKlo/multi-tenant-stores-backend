using FluentValidation;

namespace Application.Discovery.Queries.GetHomeBanners;

// Parameterless query — validator exists so ValidationBehavior always finds one.
public class GetHomeBannersValidator : AbstractValidator<GetHomeBannersQuery>
{
    public GetHomeBannersValidator() { }
}