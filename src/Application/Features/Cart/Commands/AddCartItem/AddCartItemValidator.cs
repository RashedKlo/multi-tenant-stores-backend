using FluentValidation;

namespace Application.Features.Cart.Commands.AddCartItem;

public sealed class AddCartItemValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithErrorCode("StoreId.Required")
            .WithMessage("StoreId must be a valid GUID.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithErrorCode("ProductId.Required")
            .WithMessage("ProductId must be a valid GUID.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithErrorCode("Quantity.NotPositive")
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithErrorCode("Notes.TooLong")
            .WithMessage("Notes cannot exceed 500 characters.");

        RuleForEach(x => x.OptionIds)
            .NotEmpty()
            .WithErrorCode("OptionIds.ContainsEmptyGuid")
            .WithMessage("OptionIds must contain valid GUIDs.");
    }
}
