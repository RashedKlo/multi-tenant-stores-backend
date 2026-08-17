using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Eager-loads Items -> ItemOptions and StatusHistory — backs
    // GET /api/orders/{id}.
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    // Ownership-scoped read for GET /api/orders/{id} — pass the caller's own
    // customerId so one customer can never fetch another's order by guessing ids.
    Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default);

    // Backs GET /api/orders?status=&page= ("My Orders" with filter)
    Task<(List<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        OrderStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Order order);
    void Update(Order order);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOrderItemRepository
{
    Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    void Add(OrderItem item);
    Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOrderItemOptionRepository
{
    Task<List<OrderItemOption>> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<OrderItemOption> options, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOrderStatusHistoryRepository
{
    Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    // Appends the new row. Enforcing the allowed status-transition graph
    // (e.g. Delivered -> anything is invalid) is the calling service's job,
    // not this repository's — see OrderService / status transition policy.
    Task AddAsync(OrderStatusHistory entry, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    // Looked up by the Stripe webhook handler using the intent id from the event payload.
    Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default);

    void Add(Payment payment);
    void Update(Payment payment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
