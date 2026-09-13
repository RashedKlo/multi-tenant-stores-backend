using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Features.Orders.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid? ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    IReadOnlyList<OrderItemOptionDto> Options)
{
    public static OrderItemDto FromEntity(OrderItem item, Language lang) => new(
        item.Id,
        item.ProductId,
        lang.Localize(item.NameEnSnapshot, item.NameArSnapshot),
        item.UnitPriceSnapshot,
        item.Quantity,
        item.LineTotal,
        item.Options.Select(option => OrderItemOptionDto.FromEntity(option, lang)).ToList());
}

public sealed record OrderItemOptionDto(
    Guid Id,
    string Name,
    decimal PriceAdjustment)
{
    public static OrderItemOptionDto FromEntity(OrderItemOption option, Language lang) => new(
        option.Id,
        lang.Localize(option.OptionNameEnSnapshot, option.OptionNameArSnapshot),
        option.PriceAdjustmentSnapshot);
}
