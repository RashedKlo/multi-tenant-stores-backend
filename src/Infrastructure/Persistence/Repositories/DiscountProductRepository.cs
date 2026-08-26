using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DiscountProductRepository : IDiscountProductRepository
{
    private readonly AppDbContext _context;
    public DiscountProductRepository(AppDbContext context) => _context = context;

    public Task<List<DiscountProduct>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.DiscountProducts.AsNoTracking().ToListAsync(cancellationToken);

    public Task<DiscountProduct?> GetByIdsAsync(Guid discountId, Guid productId, CancellationToken cancellationToken = default) =>
        _context.DiscountProducts
            .FirstOrDefaultAsync(dp => dp.DiscountId == discountId && dp.ProductId == productId, cancellationToken);

    public async Task AddAsync(DiscountProduct dp, CancellationToken cancellationToken = default)
    {
        // The store-match check that used to be a trigger (discount.store_id
        // must equal product.store_id) now lives here, at the single place
        // this junction row is ever created.
        var discount = await _context.Discounts.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dp.DiscountId, cancellationToken)
            ?? throw new InvalidOperationException($"Discount {dp.DiscountId} not found.");

        var product = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dp.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Product {dp.ProductId} not found.");

        if (discount.StoreId != product.StoreId)
            throw new InvalidOperationException(
                $"Discount {dp.DiscountId} and product {dp.ProductId} belong to different stores.");

        await _context.DiscountProducts.AddAsync(dp, cancellationToken);
    }

    public void Delete(DiscountProduct dp) => _context.DiscountProducts.Remove(dp);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
