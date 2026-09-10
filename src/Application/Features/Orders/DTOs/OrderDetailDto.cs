using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Orders.DTOs;

public sealed record OrderDetailDto(
    Guid Id,
    Guid StoreId,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal Total,
    string DeliveryName,
    string? DeliveryPhone,
    string DeliveryAddressText,
    decimal DeliveryLatitude,
    decimal DeliveryLongitude,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    PaymentSummaryDto? Payment)
{
    public static OrderDetailDto FromEntity(Order order) => new(
        order.Id,
        order.StoreId,
        order.Status,
        order.Subtotal,
        order.DiscountTotal,
        order.Total,
        order.DeliveryName,
        order.DeliveryPhone,
        order.DeliveryAddressText,
        order.DeliveryLatitude,
        order.DeliveryLongitude,
        order.CreatedAt,
        order.UpdatedAt,
        order.Items.Select(OrderItemDto.FromEntity).ToList(),
        order.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(OrderStatusHistoryDto.FromEntity)
            .ToList(),
        order.Payment is null
            ? null
            : new PaymentSummaryDto(
                order.Payment.Id,
                order.Payment.Status,
                order.Payment.Amount,
                order.Payment.Currency,
                order.Payment.PaidAt));
}

public sealed record PaymentSummaryDto(
    Guid Id,
    PaymentStatus Status,
    decimal Amount,
    string Currency,
    DateTime? PaidAt);
