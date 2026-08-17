namespace Application.Favorites.DTOs;

// Deliberately its own shape, not a reuse of Catalog's ProductSummaryDto —
// IsFavorite would always be a hardcoded true here, which is a smell that
// the DTO doesn't fit the context. FavoritedAt is the one field this page
// actually needs that a generic product card doesn't.
public record FavoriteProductDto(
    Guid ProductId,
    string NameEn,
    string NameAr,
    string? ThumbnailUrl,
    decimal Price,
    bool InStock,
    DateTime FavoritedAt)
{
    public static FavoriteProductDto FromEntity(Domain.Entities.FavoriteProduct f)
    {
        var p = f.Product; // always loaded — see repository's .Include(f => f.Product)
        return new(
            p.Id,
            p.NameEn,
            p.NameAr,
            p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
            p.Price,
            InStock: !p.TrackInventory || p.StockQuantity > 0,
            f.CreatedAt);
    }
}

public record FavoriteStoreDto(
    Guid StoreId,
    string NameEn,
    string NameAr,
    string? LogoUrl,
    decimal Rating,
    DateTime FavoritedAt)
{
    public static FavoriteStoreDto FromEntity(Domain.Entities.FavoriteStore f)
    {
        var s = f.Store; // always loaded — see repository's .Include(f => f.Store)
        return new(s.Id, s.NameEn, s.NameAr, s.LogoUrl, s.Rating, f.CreatedAt);
    }
}
