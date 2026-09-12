using Application.Common.Interfaces;
using Domain.Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusHandler(
    IOrderRepository orders,
    IOrderTrackingNotifier notifier)
    : IRequestHandler<ChangeOrderStatusCommand, Result>
{
    public async Task<Result> Handle(
        ChangeOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

        var result = order.ChangeStatus(request.NewStatus, request.Note, ChangedByType.Tenant);
        if (result.IsFailure)
            return result;

        await orders.SaveChangesAsync(cancellationToken);

        await notifier.NotifyStatusChangedAsync(
            order.CustomerId,
            order.Id,
            order.Status,
            request.Note,
            cancellationToken);

        return Result.Success();
    }
}