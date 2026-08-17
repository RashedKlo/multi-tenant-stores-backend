using FluentValidation;

namespace Application.Discovery.Queries.GetModuleDetail;

public class GetModuleDetailValidator : AbstractValidator<GetModuleDetailQuery>
{
      public GetModuleDetailValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithErrorCode("ModuleId.Required")
            .WithMessage("ModuleId must be a valid GUID.");
    }
}