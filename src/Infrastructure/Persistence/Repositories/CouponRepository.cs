// Infrastructure/Persistence/Repositories/CouponRepository.cs
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

    /// <summary>
    /// One UPDATE with the limit in the WHERE clause: of N concurrent checkouts fighting for the
    /// last use, exactly one gets rows == 1.
    /// </summary>
    public async Task<bool> TryRedeemAsync(
        Guid couponId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var rows = await context.Coupons
            .Where(c => c.Id == couponId &&
                        c.IsActive &&
                        c.StartsAt <= utcNow &&
                        (c.ExpiresAt == null || c.ExpiresAt >= utcNow) &&
                        (c.UsageLimitTotal == null || c.UsedCount < c.UsageLimitTotal))
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.UsedCount, c => c.UsedCount + 1),
                cancellationToken);

        return rows == 1;
    }

    public Task ReleaseRedemptionAsync(Guid couponId, CancellationToken cancellationToken = default) =>
        context.Coupons
            .Where(c => c.Id == couponId && c.UsedCount > 0)
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.UsedCount, c => c.UsedCount - 1),
                cancellationToken);
}