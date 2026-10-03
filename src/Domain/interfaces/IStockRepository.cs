// Domain/interfaces/IStockRepository.cs
using Domain.Common;

namespace Domain.Interfaces;

public sealed record StockRequest(Guid ProductId, int Quantity);

public interface IStockRepository
{
    /// <summary>
    /// Atomically decrements stock in a single statement. Fails with Conflict when any product
    /// has not enough stock. Must run inside the caller's transaction (all-or-nothing).
    /// </summary>
    /// <param name="requests">One entry per product (ProductId unique), tracked products only.</param>
    Task<Result> TryReserveAsync(
        IReadOnlyList<StockRequest> requests,
        CancellationToken cancellationToken = default);

    /// <summary>Gives back the stock of every item of the order (tracked products only).</summary>
    Task ReleaseForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}