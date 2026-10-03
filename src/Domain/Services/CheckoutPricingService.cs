// Domain/Services/CheckoutPricingService.cs
using Domain.Common;
using Domain.Entities;

namespace Domain.Services;

/// <param name="BaseUnitPrice">Product price before any product/section discount.</param>
/// <param name="FinalUnitPrice">Product price after product/section discount (before options).</param>
/// <param name="OptionsPerUnit">Sum of selected option price adjustments (options are never discounted).</param>
public sealed record PricedLine(
    decimal BaseUnitPrice,
    decimal FinalUnitPrice,
    int Quantity,
    decimal OptionsPerUnit);

public sealed record PricingResult(
    decimal Subtotal,           // before any discount: sum((base + options) * qty)
    decimal ItemDiscountTotal,  // product / section discounts
    decimal CouponDiscount)     // order-level coupon
{
    public decimal DiscountTotal => ItemDiscountTotal + CouponDiscount;
    public decimal Total => Subtotal - DiscountTotal;
}

/// <summary>
/// Pure checkout pricing rules. No I/O, trivially unit-testable.
/// Rule: item discounts first, then the coupon on the already-discounted amount.
/// </summary>
public static class CheckoutPricingService
{
    public static Result<PricingResult> Calculate(
        IReadOnlyList<PricedLine> lines,
        Coupon? coupon,
        DateTime utcNow)
    {
        if (lines.Count == 0)
            return Result<PricingResult>.Failure(
                Error.Validation("Checkout.EmptyCart", "Cart is empty."));

        decimal subtotal = 0m;
        decimal itemDiscountTotal = 0m;

        foreach (var line in lines)
        {
            if (line.Quantity <= 0 || line.FinalUnitPrice > line.BaseUnitPrice)
                return Result<PricingResult>.Failure(
                    Error.Validation("Checkout.InvalidLine", "Cart contains an invalid line."));

            subtotal += (line.BaseUnitPrice + line.OptionsPerUnit) * line.Quantity;
            itemDiscountTotal += (line.BaseUnitPrice - line.FinalUnitPrice) * line.Quantity;
        }

        decimal couponDiscount = 0m;
        if (coupon is not null)
        {
            // Coupon.CalculateDiscount validates availability + min order and caps at the amount.
            var discount = coupon.CalculateDiscount(subtotal - itemDiscountTotal, utcNow);
            if (discount.IsFailure)
                return Result<PricingResult>.Failure(discount.Errors);

            couponDiscount = discount.Value;
        }

        return Result<PricingResult>.Success(new PricingResult(subtotal, itemDiscountTotal, couponDiscount));
    }
}