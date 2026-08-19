// Application/Common/Models/CartDto.cs
using System.Text.Json.Serialization;

namespace Application.Common.Models;

public sealed record CartDto(
    Guid? CartId,
    Guid StoreId,
    List<CartItemDto> Items,
    decimal Subtotal,
    int TotalItemCount)
{
    public bool IsEmpty => CartId is null || Items.Count == 0;

    public static CartDto Empty(Guid storeId) => new(null, storeId, [], 0m, 0);
}

public sealed record CartItemDto(
    Guid CartItemId,
    Guid CartId,
    Guid ProductId,
    string ProductNameEn,
    string ProductNameAr,
    decimal BasePrice,
    int Quantity,
    string? Notes,
    IReadOnlyList<SelectedOptionDto> SelectedOptions,
    decimal ItemTotalPrice);
public sealed record SelectedOptionDto(
    [property: JsonPropertyName("option_id")] Guid OptionId,
    [property: JsonPropertyName("group_name_en")] string GroupNameEn,
    [property: JsonPropertyName("group_name_ar")] string GroupNameAr,
    [property: JsonPropertyName("option_name_en")] string OptionNameEn,
    [property: JsonPropertyName("option_name_ar")] string OptionNameAr,
    [property: JsonPropertyName("price_adjustment")] decimal PriceAdjustment);