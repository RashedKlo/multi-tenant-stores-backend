using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class OrderStatusHistory
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }
    public ChangedByType? ChangedByType { get; private set; }
    public Guid? ChangedById { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public Order Order { get; private set; } = null!;

    private OrderStatusHistory() { }

    public static Result<OrderStatusHistory> Create(
        Guid orderId,
        OrderStatus status,
        string? note = null,
        ChangedByType? changedByType = null,
        Guid? changedById = null)
    {
        if (orderId == Guid.Empty)
            return Result<OrderStatusHistory>.Failure(Error.Validation(
                "OrderStatusHistory.OrderId.Required", "OrderId is required."));

        return Result<OrderStatusHistory>.Success(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = status,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            ChangedByType = changedByType,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow
        });
    }
}