using Domain.Entities;

namespace Application.Features.Orders.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid? ProductId,
    string NameEn,
    string NameAr,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    IReadOnlyList<OrderItemOptionDto> Options)
{
    public static OrderItemDto FromEntity(OrderItem item) => new(
        item.Id,
        item.ProductId,
        item.NameEnSnapshot,
        item.NameArSnapshot,
        item.UnitPriceSnapshot,
        item.Quantity,
        item.LineTotal,
        item.Options.Select(OrderItemOptionDto.FromEntity).ToList());
}

public sealed record OrderItemOptionDto(
    Guid Id,
    string NameEn,
    string NameAr,
    decimal PriceAdjustment)
{
    public static OrderItemOptionDto FromEntity(OrderItemOption option) => new(
        option.Id,
        option.OptionNameEnSnapshot,
        option.OptionNameArSnapshot,
        option.PriceAdjustmentSnapshot);
}
