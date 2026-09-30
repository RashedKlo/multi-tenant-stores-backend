using Domain.Entities;

namespace Domain.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetActiveByStoreAndCodeAsync(
        Guid storeId,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}