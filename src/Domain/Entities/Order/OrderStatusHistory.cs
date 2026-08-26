using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class OrderStatusHistory
    {
        public Guid Id { get; private set; }

        public Guid OrderId { get; private set; }

        public OrderStatus Status { get; private set; }

        public string? Note { get; private set; }

        public ChangedByType? ChangedByType { get; private set; }

        public Guid? ChangedById { get; private set; }

        public DateTime ChangedAt { get; private set; }
public Order Order { get; private set; } = null!;
        private OrderStatusHistory()
        {
        }

        public static Result<OrderStatusHistory> Create(
            Guid orderId,
            OrderStatus status,
            string? note = null,
            ChangedByType? changedByType = null,
            Guid? changedById = null)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(orderId, errors, "OrderId");

            note = DomainValidation.NormalizeOptional(note);

            if (errors.Count > 0)
                return Result<OrderStatusHistory>.Failure(errors);

            var history = new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Status = status,
                Note = note,
                ChangedByType = changedByType,
                ChangedById = changedById,
                ChangedAt = DateTime.UtcNow
            };

            return Result<OrderStatusHistory>.Success(history);
        }
    }
}