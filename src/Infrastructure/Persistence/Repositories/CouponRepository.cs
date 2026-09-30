using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CouponRepository(AppDbContext context) : ICouponRepository
{
    public Task<Coupon?> GetActiveByStoreAndCodeAsync(
        Guid storeId,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(coupon =>
                coupon.StoreId == storeId &&
                coupon.Code == code &&
                coupon.IsActive &&
                coupon.StartsAt <= utcNow &&
                (coupon.ExpiresAt == null || coupon.ExpiresAt >= utcNow) &&
                (coupon.UsageLimitTotal == null || coupon.UsedCount < coupon.UsageLimitTotal),
                cancellationToken);
}