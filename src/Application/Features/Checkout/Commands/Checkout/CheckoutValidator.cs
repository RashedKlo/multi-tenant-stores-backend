// Application/Features/Checkout/Commands/Checkout/CheckoutValidator.cs
using FluentValidation;

namespace Application.Features.Checkout.Commands.Checkout;

public sealed class CheckoutValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.DeliveryPhone)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.DeliveryPhone));
    }
}