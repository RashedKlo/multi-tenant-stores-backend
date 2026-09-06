// Application/Common/Models/CartDto.cs
using System.Text.Json.Serialization;

namespace Application.Common.Models;

public sealed record CartItemDto(
    Guid CartItemId,
    Guid CartId,
    Guid StoreId,
    Guid ProductId,
    string ProductName,
    string ProductImage,
    decimal BasePrice,
    int Quantity,
    string? Notes,
    IReadOnlyList<SelectedOptionDto> SelectedOptions,
    decimal ItemTotalPrice);

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
    decimal UnitPrice,
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
    public decimal EffectiveUnitPrice => UnitPrice + OptionsTotal;
    public decimal LineTotal => EffectiveUnitPrice * Quantity;
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