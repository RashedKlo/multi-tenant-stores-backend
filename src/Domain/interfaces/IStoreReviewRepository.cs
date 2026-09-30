using Domain.Entities;

namespace Domain.Interfaces;

public interface IStoreReviewRepository
{
    Task<bool> ExistsByOrderIdAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default);
    Task AddAsync(StoreReview review, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}