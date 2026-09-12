using Domain.Enums;

namespace Application.Common.Interfaces;

/// <summary>
/// Pushes a real-time status update to the customer who owns the order.
/// Implemented in the Api layer with SignalR — Application only knows this interface.
/// </summary>
public interface IOrderTrackingNotifier
{
    Task NotifyStatusChangedAsync(
        Guid customerId,
        Guid orderId,
        OrderStatus newStatus,
        string? note,
        CancellationToken cancellationToken = default);
}