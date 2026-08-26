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
        _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    // GET /api/orders/{id}: Items+ItemOptions and StatusHistory are two
    // independent collections off the same root — split into two queries
    // rather than one to avoid multiplying rows.
    public Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.OrderItemOptions)
            .Include(o => o.OrderStatusHistories.OrderBy(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    // Ownership-scoped equivalent of the above — always prefer this one from
    // the API layer so a customer can never fetch another's order by id.
    public Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.OrderItemOptions)
            .Include(o => o.OrderStatusHistories.OrderBy(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, cancellationToken);

    // GET /api/orders?status=&page= — matches idx_orders_customer_status_created.
    public async Task<(List<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId, OrderStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

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

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;
    public OrderItemRepository(AppDbContext context) => _context = context;

    public Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _context.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(cancellationToken);

    public void Add(OrderItem item) => _context.OrderItems.Add(item);

    public async Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken cancellationToken = default) =>
        await _context.OrderItems.AddRangeAsync(items, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class OrderItemOptionRepository : IOrderItemOptionRepository
{
    private readonly AppDbContext _context;
    public OrderItemOptionRepository(AppDbContext context) => _context = context;

    public Task<List<OrderItemOption>> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default) =>
        _context.OrderItemOptions.AsNoTracking().Where(o => o.OrderItemId == orderItemId).ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<OrderItemOption> options, CancellationToken cancellationToken = default) =>
        await _context.OrderItemOptions.AddRangeAsync(options, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly AppDbContext _context;
    public OrderStatusHistoryRepository(AppDbContext context) => _context = context;

    public Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _context.OrderStatusHistories
            .AsNoTracking()
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(cancellationToken);

    // Transition-graph validation (e.g. Delivered -> anything is invalid)
    // is intentionally NOT here — it belongs in the order status service,
    // which reads the order's current status before calling this.
    public async Task AddAsync(OrderStatusHistory entry, CancellationToken cancellationToken = default) =>
        await _context.OrderStatusHistories.AddAsync(entry, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
