namespace Application.Features.Coupons.DTOs;

public sealed record CouponApplicationDto(
    string Code,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total);