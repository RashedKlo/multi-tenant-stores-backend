using FluentValidation;

namespace Application.Stores.Commands.CreateStore;

public class CreateStoreValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("LogoUrl must not exceed 500 characters.");

        RuleFor(x => x.BannerUrl)
            .MaximumLength(500).WithMessage("BannerUrl must not exceed 500 characters.");
    }
}
