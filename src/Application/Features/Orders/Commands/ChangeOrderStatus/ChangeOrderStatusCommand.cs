using Domain.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Commands.ChangeOrderStatus;

public sealed record ChangeOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus,
    string? Note = null) : IRequest<Result>;