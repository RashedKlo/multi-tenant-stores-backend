using FluentValidation;

namespace Application.Discovery.Queries.GetModules;

public class GetModulesValidator : AbstractValidator<GetModulesQuery>
{
    public GetModulesValidator() { }
}