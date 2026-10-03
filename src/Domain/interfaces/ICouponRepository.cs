// Domain/interfaces/ICouponRepository.cs
using Domain.Entities;

namespace Domain.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetActiveByStoreAndCodeAsync(
        Guid storeId,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomic "used_count + 1" guarded by the usage limit. Returns false when the coupon is
    /// no longer available (limit reached, expired, disabled).
    /// </summary>
    Task<bool> TryRedeemAsync(Guid couponId, DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>Atomic "used_count - 1" (never below zero). Used when an order is cancelled before payment.</summary>
    Task ReleaseRedemptionAsync(Guid couponId, CancellationToken cancellationToken = default);
}