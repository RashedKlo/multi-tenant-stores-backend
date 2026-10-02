// Application/Features/Cart/DTOs/CartDto.cs
using System.Text.Json.Serialization;

namespace Application.Features.Cart.DTOs;

/// <summary>The discount currently applied to a cart line (already validated as active).</summary>
public sealed record CartDiscountDto(
    Guid Id,
    string Type,          // "Percentage" | "FixedAmount"
    decimal Value,
    DateTime? EndsAt);

public sealed record CartItemDto(
    Guid CartItemId,
    Guid CartId,
    Guid StoreId,
    Guid ProductId,
    string ProductName,
    string? ProductImage,
    decimal BasePrice,          // price before discount
    decimal FinalUnitPrice,     // price after discount, before options
    int Quantity,
    string? Notes,
    CartDiscountDto? Discount,
    IReadOnlyList<SelectedOptionDto> SelectedOptions,
    decimal ItemTotalPrice);    // (FinalUnitPrice + options) * Quantity

public sealed record SelectedOptionDto(
    [property: JsonPropertyName("option_id")] Guid OptionId,
    [property: JsonPropertyName("group_name")] string GroupName,
    [property: JsonPropertyName("option_name")] string OptionName,
    [property: JsonPropertyName("price_adjustment")] decimal PriceAdjustment);

public sealed record CheckoutCartDto(
    Guid CartId,
    Guid StoreId,
    IReadOnlyList<CheckoutCartItemDto> Items);

public sealed record CheckoutCartItemDto(
    Guid CartItemId,
    Guid ProductId,
    string NameEn,
    string NameAr,
    decimal UnitPrice,          // price before discount
    decimal FinalUnitPrice,     // price after discount, before options
    CartDiscountDto? Discount,
    int Quantity,
    string? Notes,
    bool TrackInventory,
    int StockQuantity,
    bool IsActive,
    DateTime? DeletedAt,
    IReadOnlyList<CheckoutOptionDto> Options)
{
    public bool IsAvailable =>
        IsActive && DeletedAt is null &&
        (!TrackInventory || StockQuantity >= Quantity);

    public decimal OptionsTotal => Options.Sum(o => o.PriceAdjustment);
    public decimal EffectiveUnitPrice => FinalUnitPrice + OptionsTotal;
    public decimal LineTotal => EffectiveUnitPrice * Quantity;

    /// <summary>Total saved on this line (for order.discount_total).</summary>
    public decimal DiscountTotal => (UnitPrice - FinalUnitPrice) * Quantity;
}

public sealed record CheckoutOptionDto(
    Guid OptionId,
    string NameEn,
    string NameAr,
    decimal PriceAdjustment,
    bool IsActive,
    DateTime? DeletedAt)
{
    public bool IsAvailable => IsActive && DeletedAt is null;
}