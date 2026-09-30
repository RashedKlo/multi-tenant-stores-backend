using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class StoreReviewRepository(AppDbContext context) : IStoreReviewRepository
{
    public Task<bool> ExistsByOrderIdAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        context.StoreReviews
            .AsNoTracking()
            .AnyAsync(review => review.OrderId == orderId && review.CustomerId == customerId, cancellationToken);

    public async Task AddAsync(StoreReview review, CancellationToken cancellationToken = default) =>
        await context.StoreReviews.AddAsync(review, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}