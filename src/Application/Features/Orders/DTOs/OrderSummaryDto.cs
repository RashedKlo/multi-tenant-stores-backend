using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Orders.DTOs;

public sealed record OrderSummaryDto(
    Guid Id,
    Guid StoreId,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal Total,
    DateTime CreatedAt)
{
    public static OrderSummaryDto FromEntity(Order order) => new(
        order.Id,
        order.StoreId,
        order.Status,
        order.Subtotal,
        order.DiscountTotal,
        order.Total,
        order.CreatedAt);
}
