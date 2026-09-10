using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Orders.DTOs;

public sealed record OrderStatusHistoryDto(
    OrderStatus Status,
    string? Note,
    DateTime ChangedAt)
{
    public static OrderStatusHistoryDto FromEntity(OrderStatusHistory history) => new(
        history.Status,
        history.Note,
        history.ChangedAt);
}
