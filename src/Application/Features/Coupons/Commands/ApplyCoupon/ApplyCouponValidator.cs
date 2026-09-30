using FluentValidation;

namespace Application.Features.Coupons.Commands.ApplyCoupon;

public sealed class ApplyCouponValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);
    }
}