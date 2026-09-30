using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Coupon
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Code { get; private set; } = null!;
    public CouponDiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public int? UsageLimitTotal { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Coupon()
    {
    }

    public bool IsAvailable(DateTime utcNow) =>
        IsActive &&
        StartsAt <= utcNow &&
        (ExpiresAt is null || ExpiresAt >= utcNow) &&
        (UsageLimitTotal is null || UsedCount < UsageLimitTotal);

    public Result<decimal> CalculateDiscount(decimal subtotal, DateTime utcNow)
    {
        if (!IsAvailable(utcNow))
            return Result<decimal>.Failure(Error.Validation(
                "Coupon.Inactive", "Coupon is not currently active."));

        if (subtotal < MinOrderAmount)
            return Result<decimal>.Failure(Error.Validation(
                "Coupon.MinimumOrderNotMet",
                $"The minimum order amount for this coupon is {MinOrderAmount:0.00}."));

        var discount = DiscountType == CouponDiscountType.Percentage
            ? subtotal * DiscountValue / 100m
            : DiscountValue;

        if (MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, MaxDiscountAmount.Value);

        return Result<decimal>.Success(Math.Min(Math.Round(discount, 2), subtotal));
    }
}