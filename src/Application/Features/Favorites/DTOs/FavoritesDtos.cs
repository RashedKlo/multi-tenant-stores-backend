using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Favorites.DTOs;

public record FavoriteProductDto(
    Guid ProductId,
    string Name,
    string? ThumbnailUrl,
    decimal Price,
    bool InStock,
    DateTime FavoritedAt)
{
    public static FavoriteProductDto FromEntity(Domain.Entities.FavoriteProduct f, Language lang)
    {
        var p = f.Product;
        return new(
            p.Id,
            lang.Localize(p.NameEn, p.NameAr),
            p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
            p.Price,
            InStock: !p.TrackInventory || p.StockQuantity > 0,
            f.CreatedAt);
    }
}

public record FavoriteStoreDto(
    Guid StoreId,
    string Name,
    string? LogoUrl,
    decimal Rating,
    DateTime FavoritedAt)
{
    public static FavoriteStoreDto FromEntity(Domain.Entities.FavoriteStore f, Language lang)
    {
        var s = f.Store;
        return new(s.Id, lang.Localize(s.NameEn, s.NameAr), s.LogoUrl, s.Rating, f.CreatedAt);
    }
}