using Application.Common.Interfaces;
using Application.Features.Coupons.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Coupons.Commands.ApplyCoupon;

public sealed class ApplyCouponHandler(
    ICouponRepository coupons,
    ICartQueries cartQueries,
    ICurrentUserService currentUser)
    : IRequestHandler<ApplyCouponCommand, Result<CouponApplicationDto>>
{
    public async Task<Result<CouponApplicationDto>> Handle(
        ApplyCouponCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<CouponApplicationDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var coupon = await coupons.GetActiveByStoreAndCodeAsync(
            request.StoreId,
            request.Code.Trim(),
            DateTime.UtcNow,
            cancellationToken);

        if (coupon is null)
            return Result<CouponApplicationDto>.Failure(
                Error.NotFound("Coupon.NotFound", "Coupon was not found or is no longer active."));

        var cart = await cartQueries.GetCartForCheckoutAsync(
            currentUser.CustomerId.Value,
            request.StoreId,
            cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            return Result<CouponApplicationDto>.Failure(
                Error.Validation("Coupon.EmptyCart", "Cart is empty for this store."));

        var subtotal = cart.Items.Sum(item => item.LineTotal);
        var discountResult = coupon.CalculateDiscount(subtotal, DateTime.UtcNow);

        if (discountResult.IsFailure)
            return Result<CouponApplicationDto>.Failure(discountResult.Errors);

        var discount = discountResult.Value;
        return Result<CouponApplicationDto>.Success(new CouponApplicationDto(
            coupon.Code,
            subtotal,
            discount,
            subtotal - discount));
    }
}