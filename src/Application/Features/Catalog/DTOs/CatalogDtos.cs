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
    int ReviewCount,      
    bool IsFavorite);
public record StoreBannerDto(Guid Id, string ImageUrl, string? Title, string? ActionUrl);

public record StoreCouponDto(
    Guid Id,
    string Code,
    short DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal MinOrderAmount,
    DateTime StartsAt,
    DateTime? ExpiresAt);

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