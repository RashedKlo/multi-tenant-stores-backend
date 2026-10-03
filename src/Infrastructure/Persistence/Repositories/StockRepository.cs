// Infrastructure/Persistence/Repositories/StockRepository.cs
using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// One SQL round-trip per operation, regardless of how many products are in the cart.
/// Runs on the DbContext connection, so it joins the transaction opened by IUnitOfWork.
/// </summary>
public sealed class StockRepository(AppDbContext db) : IStockRepository
{
    /// <remarks>
    /// Contract: <paramref name="requests"/> holds one entry per product (ProductId unique) and
    /// tracked products only. The checkout handler already guarantees this.
    /// All-or-nothing relies on the caller: when this returns a failure, the UnitOfWork rolls the
    /// transaction back, which undoes any rows already decremented by this statement.
    /// Do not call it outside IUnitOfWork.ExecuteInTransactionAsync.
    /// </remarks>
    public async Task<Result> TryReserveAsync(
        IReadOnlyList<StockRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
            return Result.Success();

        var ids = requests.Select(r => r.ProductId).ToArray();
        var quantities = requests.Select(r => r.Quantity).ToArray();

        // The CTE locks the rows in id order first, so two checkouts with overlapping carts
        // queue up instead of deadlocking. The UPDATE then re-checks the stock under the lock.
        var updated = await db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH locked AS (
                SELECT id
                FROM products
                WHERE id = ANY({ids})
                ORDER BY id
                FOR UPDATE
            )
            UPDATE products p
            SET stock_quantity = p.stock_quantity - r.qty
            FROM unnest({ids}, {quantities}) AS r(product_id, qty),
                 locked l
            WHERE p.id = r.product_id
              AND l.id = p.id
              AND p.track_inventory = true
              AND p.stock_quantity >= r.qty
            """, cancellationToken);

        // Every requested product must have been updated, otherwise at least one is short.
        return updated == ids.Length
            ? Result.Success()
            : Result.Failure(Error.Conflict(
                "Checkout.OutOfStock",
                "One or more items just went out of stock. Please review your cart."));
    }

    // Already a single statement (aggregate + UPDATE ... FROM), no loop needed.
    public async Task ReleaseForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE products p
            SET stock_quantity = p.stock_quantity + oi.qty::int
            FROM (
                SELECT product_id, SUM(quantity) AS qty
                FROM order_items
                WHERE order_id = {orderId} AND product_id IS NOT NULL
                GROUP BY product_id
            ) oi
            WHERE p.id = oi.product_id AND p.track_inventory = true
            """, cancellationToken);
}