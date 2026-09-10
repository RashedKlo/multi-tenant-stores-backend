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
        s.Id, lang.Localize(s.NameEn, s.NameAr), lang.LocalizeNullable(s.DescriptionEn, s.DescriptionAr),
        s.LogoUrl, s.BannerUrl, s.Phone, s.Rating, s.Latitude, s.Longitude, isFavorite);
}

public record StoreBannerDto(Guid Id, string ImageUrl, string? Title, string? ActionUrl)
{
    public static StoreBannerDto FromEntity(Domain.Entities.StoreBanner b, Language lang) => new(
        b.Id, b.ImageUrl, lang.LocalizeNullable(b.TitleEn, b.TitleAr), b.ActionUrl);
}

public record StoreSectionDto(Guid Id, string Name, string? ImageUrl)
{
    public static StoreSectionDto FromEntity(Domain.Entities.StoreSection s, Language lang) => new(
        s.Id, lang.Localize(s.NameEn, s.NameAr), s.ImageUrl);
}

// Listing-card shape — deliberately excludes description, full image gallery,
// and option groups. Those only matter once a customer opens the product,
// which is GetProductByIdQuery, not this one.
public record ProductSummaryDto(
    Guid Id,
    string Name,
    string? ThumbnailUrl,
    decimal Price,
    decimal? ComparePrice,
    bool InStock)
{
    public static ProductSummaryDto FromEntity(Domain.Entities.Product p, Language lang) => new(
        p.Id, lang.Localize(p.NameEn, p.NameAr),
        p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
        p.Price, p.ComparePrice,
        InStock: !p.TrackInventory || p.StockQuantity > 0);
}

public record ProductImageDto(Guid Id, string ImageUrl)
{
    public static ProductImageDto FromEntity(Domain.Entities.ProductImage i) => new(i.Id, i.ImageUrl);
}

public record ProductOptionDto(Guid Id, string Name, decimal PriceAdjustment, bool IsDefault)
{
    public static ProductOptionDto FromEntity(Domain.Entities.ProductOption o, Language lang) => new(
        o.Id, lang.Localize(o.NameEn, o.NameAr), o.PriceAdjustment, o.IsDefault);
}

public record ProductOptionGroupDto(
    Guid Id,
    string Name,
    string SelectionType,
    int MinSelection,
    int MaxSelection,
    List<ProductOptionDto> Options)
{

}

// The full "precomputed JSON" shape for the product detail page — everything
// the option-picker UI needs in one response, no follow-up calls.
public record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal? ComparePrice,
    bool InStock,
    int? StockQuantity,
    bool IsFavorite,
    List<ProductImageDto> Images,
    List<ProductOptionGroupDto> OptionGroups)
{
 }