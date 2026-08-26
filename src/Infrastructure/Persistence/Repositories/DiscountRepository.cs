using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly AppDbContext _context;
    public DiscountRepository(AppDbContext context) => _context = context;

    public Task<Discount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Discounts.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    // Matches idx_discounts_active_window (store_id, start_date, end_date)
    // WHERE is_active — keep this predicate shape so Postgres uses it.
    public Task<List<Discount>> GetActiveByStoreIdAsync(Guid storeId, DateTimeOffset asOf, CancellationToken cancellationToken = default) =>
        _context.Discounts
            .AsNoTracking()
            .Where(d => d.StoreId == storeId
                     && d.IsActive
                     && (d.StartDate == null || d.StartDate <= asOf)
                     && (d.EndDate == null || d.EndDate >= asOf))
            .ToListAsync(cancellationToken);

    public void Add(Discount discount) => _context.Discounts.Add(discount);
    public void Update(Discount discount) => _context.Discounts.Update(discount);
    public void Delete(Discount discount) => _context.Discounts.Remove(discount);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
