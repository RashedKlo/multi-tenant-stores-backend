namespace Application.Catalog.DTOs;

public record StoreDetailDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    string? LogoUrl,
    string? BannerUrl,
    string? Phone,
    decimal Rating,
    decimal? Latitude,
    decimal? Longitude,
    bool IsFavorite)
{
    public static StoreDetailDto FromEntity(Domain.Entities.Store s, bool isFavorite) => new(
        s.Id, s.NameEn, s.NameAr, s.DescriptionEn, s.DescriptionAr,
        s.LogoUrl, s.BannerUrl, s.Phone, s.Rating, s.Latitude, s.Longitude, isFavorite);
}

public record StoreBannerDto(Guid Id, string ImageUrl, string? TitleEn, string? TitleAr, string? ActionUrl)
{
    public static StoreBannerDto FromEntity(Domain.Entities.StoreBanner b) => new(
        b.Id, b.ImageUrl, b.TitleEn, b.TitleAr, b.ActionUrl);
}

public record StoreSectionDto(Guid Id, string NameEn, string NameAr, string? ImageUrl)
{
    public static StoreSectionDto FromEntity(Domain.Entities.StoreSection s) => new(
        s.Id, s.NameEn, s.NameAr, s.ImageUrl);
}

// Listing-card shape — deliberately excludes description, full image gallery,
// and option groups. Those only matter once a customer opens the product,
// which is GetProductByIdQuery, not this one.
public record ProductSummaryDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? ThumbnailUrl,
    decimal Price,
    decimal? ComparePrice,
    bool InStock)
{
    public static ProductSummaryDto FromEntity(Domain.Entities.Product p) => new(
        p.Id, p.NameEn, p.NameAr,
        p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
        p.Price, p.ComparePrice,
        InStock: !p.TrackInventory || p.StockQuantity > 0);
}

public record ProductImageDto(Guid Id, string ImageUrl)
{
    public static ProductImageDto FromEntity(Domain.Entities.ProductImage i) => new(i.Id, i.ImageUrl);
}

public record ProductOptionDto(Guid Id, string NameEn, string NameAr, decimal PriceAdjustment, bool IsDefault)
{
    public static ProductOptionDto FromEntity(Domain.Entities.ProductOption o) => new(
        o.Id, o.NameEn, o.NameAr, o.PriceAdjustment, o.IsDefault);
}

public record ProductOptionGroupDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string SelectionType,
    int MinSelection,
    int MaxSelection,
    List<ProductOptionDto> Options)
{
    public static ProductOptionGroupDto FromEntity(Domain.Entities.ProductOptionGroup g) => new(
        g.Id, g.NameEn, g.NameAr, g.SelectionType.ToString(), g.MinSelection, g.MaxSelection,
        g.Options.Select(ProductOptionDto.FromEntity).ToList());
}

// The full "precomputed JSON" shape for the product detail page — everything
// the option-picker UI needs in one response, no follow-up calls.
public record ProductDetailDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal Price,
    decimal? ComparePrice,
    bool InStock,
    int? StockQuantity,
    bool IsFavorite,
    List<ProductImageDto> Images,
    List<ProductOptionGroupDto> OptionGroups)
{
    public static ProductDetailDto FromEntity(Domain.Entities.Product p, bool isFavorite) => new(
        p.Id, p.NameEn, p.NameAr, p.DescriptionEn, p.DescriptionAr,
        p.Price, p.ComparePrice,
        InStock: !p.TrackInventory || p.StockQuantity > 0,
        // Only expose the raw count when the store actually tracks it —
        // an untracked product's stock_quantity column is meaningless noise.
        StockQuantity: p.TrackInventory ? p.StockQuantity : null,
        isFavorite,
        p.Images.OrderBy(i => i.DisplayOrder).Select(ProductImageDto.FromEntity).ToList(),
        p.OptionGroups.OrderBy(g => g.DisplayOrder).Select(ProductOptionGroupDto.FromEntity).ToList());
}
