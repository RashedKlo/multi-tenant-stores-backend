using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public sealed class OrderTrackingNotifier(IHubContext<OrderTrackingHub> hub) : IOrderTrackingNotifier
{
    public Task NotifyStatusChangedAsync(
        Guid customerId,
        Guid orderId,
        OrderStatus newStatus,
        string? note,
        CancellationToken cancellationToken = default)
        => hub.Clients.User(customerId.ToString()).SendAsync(
            "OrderStatusChanged",
            new { orderId, status = newStatus.ToString(), note, changedAt = DateTime.UtcNow },
            cancellationToken);
}