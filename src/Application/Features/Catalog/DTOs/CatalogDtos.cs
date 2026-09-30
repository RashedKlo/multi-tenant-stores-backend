using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Catalog.DTOs;

public record StoreDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    string? Phone,
    decimal Rating,
    decimal? Latitude,
    decimal? Longitude,
    bool IsFavorite)
{
    public static StoreDetailDto FromEntity(Domain.Entities.Store s, bool isFavorite, Language lang) => new(
        s.Id,
        lang.Localize(s.NameEn, s.NameAr),
        lang.LocalizeNullable(s.DescriptionEn, s.DescriptionAr),
        s.LogoUrl,
        s.BannerUrl,
        s.Phone,
        s.Rating,
        s.Latitude,
        s.Longitude,
        isFavorite);
}

public record StoreBannerDto(Guid Id, string ImageUrl, string? Title, string? ActionUrl);

public record DiscountInfoDto(
    string Type,
    decimal Value,
    DateTime? EndsAt,
    string Source);

public record StoreSectionDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    DiscountInfoDto? Discount);

public record ProductSummaryDto(
    Guid Id,
    string Name,
    string? ThumbnailUrl,
    decimal Price,
    decimal FinalPrice,
    decimal? ComparePrice,
    bool InStock,
    DiscountInfoDto? Discount);


public record ProductImageDto(Guid Id, string ImageUrl);
public record ProductOptionDto(Guid Id, string Name, decimal PriceAdjustment, bool IsDefault);

public record ProductOptionGroupDto(
    Guid Id,
    string Name,
    string SelectionType,
    int MinSelection,
    int MaxSelection,
    List<ProductOptionDto> Options);
public record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal FinalPrice,
    decimal? ComparePrice,
    bool InStock,
    int? StockQuantity,
    bool IsFavorite,
    DiscountInfoDto? Discount,
    List<ProductImageDto> Images,
    List<ProductOptionGroupDto> OptionGroups);