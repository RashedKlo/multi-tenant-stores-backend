using FluentValidation;

namespace Application.Features.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}