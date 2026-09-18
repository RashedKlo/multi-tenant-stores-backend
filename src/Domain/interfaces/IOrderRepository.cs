// Domain/Interfaces/OrderRepositoryInterfaces.cs
using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdForCustomerAsync(
        Guid id, Guid customerId, CancellationToken cancellationToken = default);

    Task<(List<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        OrderStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
