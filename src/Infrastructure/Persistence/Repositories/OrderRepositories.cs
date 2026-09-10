using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) => _context = context;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Items)
            .ThenInclude(i => i.Options)
            .Include(o => o.StatusHistory)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Items)
            .ThenInclude(i => i.Options)
            .Include(o => o.StatusHistory)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, cancellationToken);

    public async Task<(List<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        OrderStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Order order) => _context.Orders.Add(order);
    public void Update(Order order) => _context.Orders.Update(order);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
