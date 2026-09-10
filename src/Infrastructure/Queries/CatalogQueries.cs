using Application.Catalog.DTOs;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Dapper;
using Infrastructure.Persistence;

namespace Infrastructure.Queries;

public sealed class CatalogQueries : ICatalogQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogQueries(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<StoreDetailDto?> GetStoreByIdAsync(
        Guid storeId,
        Guid? customerId,
        Language lang,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                s.id,
                s.name_en,
                s.name_ar,
                s.description_en,
                s.description_ar,
                s.logo_url,
                s.banner_url,
                s.phone,
                s.rating,
                s.latitude,
                s.longitude,
                EXISTS (
                    SELECT 1 FROM favorite_stores fs
                    WHERE fs.store_id = s.id
                      AND fs.customer_id = @CustomerId
                ) AS is_favorite
            FROM stores s
            WHERE s.id = @StoreId
              AND s.is_active = true
              AND s.deleted_at IS NULL
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<StoreRow>(
            new CommandDefinition(sql, new { StoreId = storeId, CustomerId = customerId }, cancellationToken: ct));

        if (row is null) return null;

        return new StoreDetailDto(
            row.Id,
            lang.Localize(row.NameEn, row.NameAr),
            lang.LocalizeNullable(row.DescriptionEn, row.DescriptionAr),
            row.LogoUrl,
            row.BannerUrl,
            row.Phone,
            row.Rating,
            row.Latitude,
            row.Longitude,
            row.IsFavorite);
    }

    public async Task<IReadOnlyList<StoreBannerDto>> GetStoreBannersAsync(
        Guid storeId,
        Language lang,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, image_url, title_en, title_ar, action_url
            FROM store_banners
            WHERE store_id = @StoreId AND is_active = true
            ORDER BY display_order, id
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();
        var rows = await conn.QueryAsync<StoreBannerRow>(
            new CommandDefinition(sql, new { StoreId = storeId }, cancellationToken: ct));

        return rows.Select(r => new StoreBannerDto(
            r.Id,
            r.ImageUrl,
            lang.LocalizeNullable(r.TitleEn, r.TitleAr),
            r.ActionUrl)).ToList();
    }

    public async Task<PagedResult<StoreSectionDto>> GetStoreSectionsAsync(
        Guid storeId,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default)
    {
        const string countSql = """
            SELECT COUNT(*) FROM store_sections
            WHERE store_id = @StoreId AND is_active = true
            """;

        const string dataSql = """
            SELECT id, name_en, name_ar, image_url
            FROM store_sections
            WHERE store_id = @StoreId AND is_active = true
            ORDER BY display_order, id
            OFFSET @Offset LIMIT @PageSize
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, new { StoreId = storeId }, cancellationToken: ct));

        var rows = await conn.QueryAsync<StoreSectionRow>(
            new CommandDefinition(dataSql, new
            {
                StoreId = storeId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: ct));

        var items = rows.Select(r => new StoreSectionDto(
            r.Id,
            lang.Localize(r.NameEn, r.NameAr),
            r.ImageUrl)).ToList();

        return PagedResult<StoreSectionDto>.Create(items, page, pageSize, total);
    }

    public async Task<PagedResult<ProductSummaryDto>> GetProductsBySectionAsync(
        Guid sectionId,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default)
    {
        var filters = new List<string>
        {
            "p.section_id = @SectionId",
            "p.is_active = true",
            "p.deleted_at IS NULL"
        };

        if (inStockOnly == true)
            filters.Add("(p.track_inventory = false OR p.stock_quantity > 0)");

        if (minPrice.HasValue)
            filters.Add("p.price >= @MinPrice");

        if (maxPrice.HasValue)
            filters.Add("p.price <= @MaxPrice");

        var where = string.Join(" AND ", filters);

        var countSql = $"""
            SELECT COUNT(*) FROM products p WHERE {where}
            """;

        var dataSql = $"""
            SELECT
                p.id,
                p.name_en,
                p.name_ar,
                p.price,
                p.compare_price,
                p.track_inventory,
                p.stock_quantity,
                (
                    SELECT pi.image_url
                    FROM product_images pi
                    WHERE pi.product_id = p.id
                    ORDER BY pi.display_order
                    LIMIT 1
                ) AS thumbnail_url
            FROM products p
            WHERE {where}
            ORDER BY p.created_at DESC
            OFFSET @Offset LIMIT @PageSize
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var param = new
        {
            SectionId = sectionId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: ct));

        var rows = await conn.QueryAsync<ProductSummaryRow>(
            new CommandDefinition(dataSql, param, cancellationToken: ct));

        var items = rows.Select(r => new ProductSummaryDto(
            r.Id,
            lang.Localize(r.NameEn, r.NameAr),
            r.ThumbnailUrl,
            r.Price,
            r.ComparePrice,
            InStock: !r.TrackInventory || r.StockQuantity > 0)).ToList();

        return PagedResult<ProductSummaryDto>.Create(items, page, pageSize, total);
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(
        Guid productId,
        Guid? customerId,
        Language lang,
        CancellationToken ct = default)
    {
        const string productSql = """
            SELECT
                p.id,
                p.name_en,
                p.name_ar,
                p.description_en,
                p.description_ar,
                p.price,
                p.compare_price,
                p.track_inventory,
                p.stock_quantity,
                EXISTS (
                    SELECT 1 FROM favorite_products fp
                    WHERE fp.product_id = p.id
                      AND fp.customer_id = @CustomerId
                ) AS is_favorite
            FROM products p
            WHERE p.id = @ProductId
              AND p.is_active = true
              AND p.deleted_at IS NULL
            """;

        const string imagesSql = """
            SELECT id, image_url
            FROM product_images
            WHERE product_id = @ProductId
            ORDER BY display_order, id
            """;

        const string groupsSql = """
            SELECT
                g.id,
                g.name_en,
                g.name_ar,
                g.selection_type::text AS selection_type,
                g.min_selection,
                g.max_selection,
                g.display_order
            FROM product_option_groups g
            WHERE g.product_id = @ProductId
              AND g.is_active = true
              AND g.deleted_at IS NULL
            ORDER BY g.display_order, g.id
            """;

        const string optionsSql = """
            SELECT
                o.id,
                o.option_group_id,
                o.name_en,
                o.name_ar,
                o.price_adjustment,
                o.is_default,
                o.display_order
            FROM product_options o
            JOIN product_option_groups g ON g.id = o.option_group_id
            WHERE g.product_id = @ProductId
              AND o.is_active = true
              AND o.deleted_at IS NULL
            ORDER BY o.display_order, o.id
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var product = await conn.QuerySingleOrDefaultAsync<ProductDetailRow>(
            new CommandDefinition(productSql, new { ProductId = productId, CustomerId = customerId }, cancellationToken: ct));

        if (product is null) return null;

        var images = (await conn.QueryAsync<ProductImageRow>(
            new CommandDefinition(imagesSql, new { ProductId = productId }, cancellationToken: ct))).ToList();

        var groups = (await conn.QueryAsync<OptionGroupRow>(
            new CommandDefinition(groupsSql, new { ProductId = productId }, cancellationToken: ct))).ToList();

        var options = (await conn.QueryAsync<OptionRow>(
            new CommandDefinition(optionsSql, new { ProductId = productId }, cancellationToken: ct))).ToList();

        var optionGroups = groups.Select(g => new ProductOptionGroupDto(
            g.Id,
            lang.Localize(g.NameEn, g.NameAr),
            g.SelectionType,
            g.MinSelection,
            g.MaxSelection,
            options
                .Where(o => o.OptionGroupId == g.Id)
                .Select(o => new ProductOptionDto(
                    o.Id,
                    lang.Localize(o.NameEn, o.NameAr),
                    o.PriceAdjustment,
                    o.IsDefault))
                .ToList())).ToList();

        return new ProductDetailDto(
            product.Id,
            lang.Localize(product.NameEn, product.NameAr),
            lang.LocalizeNullable(product.DescriptionEn, product.DescriptionAr),
            product.Price,
            product.ComparePrice,
            InStock: !product.TrackInventory || product.StockQuantity > 0,
            StockQuantity: product.TrackInventory ? product.StockQuantity : null,
            product.IsFavorite,
            images.Select(i => new ProductImageDto(i.Id, i.ImageUrl)).ToList(),
            optionGroups);
    }

    private sealed class StoreRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public string? LogoUrl { get; init; }
        public string? BannerUrl { get; init; }
        public string? Phone { get; init; }
        public decimal Rating { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public bool IsFavorite { get; init; }
    }

    private sealed class StoreBannerRow
    {
        public Guid Id { get; init; }
        public string ImageUrl { get; init; } = default!;
        public string? TitleEn { get; init; }
        public string? TitleAr { get; init; }
        public string? ActionUrl { get; init; }
    }

    private sealed class StoreSectionRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? ImageUrl { get; init; }
    }

    private sealed class ProductSummaryRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public bool TrackInventory { get; init; }
        public int StockQuantity { get; init; }
        public string? ThumbnailUrl { get; init; }
    }

    private sealed class ProductDetailRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public bool TrackInventory { get; init; }
        public int StockQuantity { get; init; }
        public bool IsFavorite { get; init; }
    }

    private sealed class ProductImageRow
    {
        public Guid Id { get; init; }
        public string ImageUrl { get; init; } = default!;
    }

    private sealed class OptionGroupRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string SelectionType { get; init; } = default!;
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }
        public int DisplayOrder { get; init; }
    }

    private sealed class OptionRow
    {
        public Guid Id { get; init; }
        public Guid OptionGroupId { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public decimal PriceAdjustment { get; init; }
        public bool IsDefault { get; init; }
        public int DisplayOrder { get; init; }
    }
}
