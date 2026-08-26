using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DiscountSectionRepository : IDiscountSectionRepository
{
    private readonly AppDbContext _context;
    public DiscountSectionRepository(AppDbContext context) => _context = context;

    public Task<List<DiscountSection>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.DiscountSections.AsNoTracking().ToListAsync(cancellationToken);

    public Task<DiscountSection?> GetByIdsAsync(Guid discountId, Guid sectionId, CancellationToken cancellationToken = default) =>
        _context.DiscountSections
            .FirstOrDefaultAsync(ds => ds.DiscountId == discountId && ds.SectionId == sectionId, cancellationToken);

    public async Task AddAsync(DiscountSection ds, CancellationToken cancellationToken = default)
    {
        var discount = await _context.Discounts.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == ds.DiscountId, cancellationToken)
            ?? throw new InvalidOperationException($"Discount {ds.DiscountId} not found.");

        var section = await _context.StoreSections.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == ds.SectionId, cancellationToken)
            ?? throw new InvalidOperationException($"Section {ds.SectionId} not found.");

        if (discount.StoreId != section.StoreId)
            throw new InvalidOperationException(
                $"Discount {ds.DiscountId} and section {ds.SectionId} belong to different stores.");

        await _context.DiscountSections.AddAsync(ds, cancellationToken);
    }

    public void Delete(DiscountSection ds) => _context.DiscountSections.Remove(ds);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
