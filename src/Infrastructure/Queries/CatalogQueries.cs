using System.Data.Common;
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

    private DbConnection CreateConnection()
        => (DbConnection)_connectionFactory.CreateConnection();

    // ════════════════════════════════════════════════════════════════════
    //  STORE
    // ════════════════════════════════════════════════════════════════════

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
                s.latitude,
                s.longitude,
                COALESCE(rv.rating_avg, 0)  AS rating_avg,
                COALESCE(rv.review_count, 0) AS review_count,
                EXISTS (
                    SELECT 1
                    FROM favorite_stores fs
                    WHERE fs.store_id = s.id
                      AND fs.customer_id = @CustomerId::uuid
                ) AS is_favorite
            FROM stores s
            LEFT JOIN LATERAL (
                SELECT ROUND(AVG(r.rating)::numeric, 1) AS rating_avg,
                       COUNT(*)::int                    AS review_count
                FROM store_reviews r
                WHERE r.store_id = s.id
            ) rv ON true
            WHERE s.id = @StoreId
              AND s.is_active = true
              AND s.deleted_at IS NULL
            """;

        await using var conn = CreateConnection();

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
            row.RatingAvg,
            row.Latitude,
            row.Longitude,
            row.ReviewCount,
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
            WHERE store_id = @StoreId
              AND is_active = true
            ORDER BY display_order, id
            """;

        await using var conn = CreateConnection();

        var rows = await conn.QueryAsync<StoreBannerRow>(
            new CommandDefinition(sql, new { StoreId = storeId }, cancellationToken: ct));

        return rows
            .Select(r => new StoreBannerDto(
                r.Id,
                r.ImageUrl,
                lang.LocalizeNullable(r.TitleEn, r.TitleAr),
                r.ActionUrl))
            .ToList();
    }

    public async Task<IReadOnlyList<StoreCouponDto>> GetStoreCouponsAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                id,
                code,
                discount_type,
                discount_value,
                max_discount_amount,
                min_order_amount,
                starts_at,
                expires_at
            FROM coupons
            WHERE store_id = @StoreId
              AND is_active = true
              AND starts_at <= NOW()
              AND (expires_at IS NULL OR expires_at >= NOW())
              AND (usage_limit_total IS NULL OR used_count < usage_limit_total)
            ORDER BY starts_at DESC, id
            """;

        await using var conn = CreateConnection();

        var rows = await conn.QueryAsync<StoreCouponRow>(
            new CommandDefinition(sql, new { StoreId = storeId }, cancellationToken: ct));

        return rows
            .Select(r => new StoreCouponDto(
                r.Id,
                r.Code,
                r.DiscountType,
                r.DiscountValue,
                r.MaxDiscountAmount,
                r.MinOrderAmount,
                r.StartsAt,
                r.ExpiresAt))
            .ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    //  SECTIONS
    // ════════════════════════════════════════════════════════════════════

    public async Task<PagedResult<StoreSectionDto>> GetStoreSectionsAsync(
        Guid storeId,
        bool discountedOnly,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default)
    {
        var discountFilter = discountedOnly ? "AND sd.discount_id IS NOT NULL" : string.Empty;

        // The count uses the same join + filter as the data query,
        // so `total` stays correct when discountedOnly = true.
        var countSql = $"""
            SELECT COUNT(*)
            FROM store_sections s
            {Sql.SectionDiscountJoin}
            WHERE s.store_id = @StoreId
              AND s.is_active = true
              {discountFilter}
            """;

        var dataSql = $"""
            SELECT
                s.id,
                s.name_en,
                s.name_ar,
                s.image_url,
                sd.discount_id,
                sd.discount_type,
                sd.discount_value,
                sd.discount_ends_at
            FROM store_sections s
            {Sql.SectionDiscountJoin}
            WHERE s.store_id = @StoreId
              AND s.is_active = true
              {discountFilter}
            ORDER BY s.display_order, s.id
            OFFSET @Offset LIMIT @PageSize
            """;

        var parameters = new
        {
            StoreId = storeId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        await using var conn = CreateConnection();

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var rows = await conn.QueryAsync<StoreSectionRow>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        var items = rows
            .Select(r => new StoreSectionDto(
                r.Id,
                lang.Localize(r.NameEn, r.NameAr),
                r.ImageUrl,
                r.ToDiscount(DiscountSources.Section)))
            .ToList();

        return PagedResult<StoreSectionDto>.Create(items, page, pageSize, total);
    }

    // ════════════════════════════════════════════════════════════════════
    //  PRODUCTS
    // ════════════════════════════════════════════════════════════════════

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

        // Only static fragments are concatenated; all values go through parameters.
        var where = string.Join("\n  AND ", filters);

        var countSql = $"""
            SELECT COUNT(*)
            FROM products p
            WHERE {where}
            """;

        var dataSql = $"""
            SELECT {Sql.ProductSummaryColumns}
            FROM products p
            {Sql.ProductDiscountJoin}
            WHERE {where}
            ORDER BY p.created_at DESC, p.id
            OFFSET @Offset LIMIT @PageSize
            """;

        var parameters = new
        {
            SectionId = sectionId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        return await QueryProductPageAsync(countSql, dataSql, parameters, page, pageSize, lang, ct);
    }

    public async Task<PagedResult<ProductSummaryDto>> GetDiscountedProductsAsync(
        Guid storeId,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default)
    {
        const string where = """
            p.store_id = @StoreId
            AND p.is_active = true
            AND p.deleted_at IS NULL
            AND bd.discount_id IS NOT NULL
            """;

        var countSql = $"""
            SELECT COUNT(*)
            FROM products p
            {Sql.ProductDiscountJoin}
            WHERE {where}
            """;

        // Biggest absolute saving first.
        var dataSql = $"""
            SELECT {Sql.ProductSummaryColumns}
            FROM products p
            {Sql.ProductDiscountJoin}
            WHERE {where}
            ORDER BY (p.price - bd.final_price) DESC, p.created_at DESC, p.id
            OFFSET @Offset LIMIT @PageSize
            """;

        var parameters = new
        {
            StoreId = storeId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        return await QueryProductPageAsync(countSql, dataSql, parameters, page, pageSize, lang, ct);
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(
        Guid productId,
        Guid? customerId,
        Language lang,
        CancellationToken ct = default)
    {
        var productSql = $"""
            SELECT
                p.id,
                p.name_en,
                p.name_ar,
                p.description_en,
                p.description_ar,
                p.price,
                COALESCE(bd.final_price, p.price) AS final_price,
                p.compare_price,
                p.track_inventory,
                p.stock_quantity,
                bd.discount_id,
                bd.discount_type,
                bd.discount_value,
                bd.discount_ends_at,
                bd.discount_source,
                EXISTS (
                    SELECT 1
                    FROM favorite_products fp
                    WHERE fp.product_id = p.id
                      AND fp.customer_id = @CustomerId::uuid
                ) AS is_favorite
            FROM products p
            {Sql.ProductDiscountJoin}
            WHERE p.id = @ProductId
              AND p.is_active = true
              AND p.deleted_at IS NULL
            """;

        // Three small child queries, executed in a single round-trip.
        const string childrenSql = """
            SELECT id, image_url
            FROM product_images
            WHERE product_id = @ProductId
            ORDER BY display_order, id;

            SELECT
                g.id,
                g.name_en,
                g.name_ar,
                g.selection_type::text AS selection_type,
                g.min_selection,
                g.max_selection
            FROM product_option_groups g
            WHERE g.product_id = @ProductId
              AND g.is_active = true
              AND g.deleted_at IS NULL
            ORDER BY g.display_order, g.id;

            SELECT
                o.id,
                o.option_group_id,
                o.name_en,
                o.name_ar,
                o.price_adjustment,
                o.is_default
            FROM product_options o
            JOIN product_option_groups g ON g.id = o.option_group_id
            WHERE g.product_id = @ProductId
              AND g.is_active = true
              AND g.deleted_at IS NULL
              AND o.is_active = true
              AND o.deleted_at IS NULL
            ORDER BY o.display_order, o.id;
            """;

        await using var conn = CreateConnection();

        var product = await conn.QuerySingleOrDefaultAsync<ProductDetailRow>(
            new CommandDefinition(
                productSql,
                new { ProductId = productId, CustomerId = customerId },
                cancellationToken: ct));

        if (product is null) return null;

        await using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(childrenSql, new { ProductId = productId }, cancellationToken: ct));

        var images = (await multi.ReadAsync<ProductImageRow>()).ToList();
        var groups = (await multi.ReadAsync<OptionGroupRow>()).ToList();
        var options = (await multi.ReadAsync<OptionRow>()).ToLookup(o => o.OptionGroupId);

        var optionGroups = groups
            .Select(g => new ProductOptionGroupDto(
                g.Id,
                lang.Localize(g.NameEn, g.NameAr),
                g.SelectionType,
                g.MinSelection,
                g.MaxSelection,
                options[g.Id]
                    .Select(o => new ProductOptionDto(
                        o.Id,
                        lang.Localize(o.NameEn, o.NameAr),
                        o.PriceAdjustment,
                        o.IsDefault))
                    .ToList()))
            .ToList();

        return new ProductDetailDto(
            product.Id,
            lang.Localize(product.NameEn, product.NameAr),
            lang.LocalizeNullable(product.DescriptionEn, product.DescriptionAr),
            product.Price,
            product.FinalPrice,
            product.ComparePrice,
            InStock: IsInStock(product.TrackInventory, product.StockQuantity),
            StockQuantity: product.TrackInventory ? product.StockQuantity : null,
            product.IsFavorite,
            product.ToDiscount(DiscountSources.Section),
            images.Select(i => new ProductImageDto(i.Id, i.ImageUrl)).ToList(),
            optionGroups);
    }

    // ════════════════════════════════════════════════════════════════════
    //  SHARED HELPERS
    // ════════════════════════════════════════════════════════════════════

    private async Task<PagedResult<ProductSummaryDto>> QueryProductPageAsync(
        string countSql,
        string dataSql,
        object parameters,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct)
    {
        await using var conn = CreateConnection();

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var rows = await conn.QueryAsync<ProductSummaryRow>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        var items = rows
            .Select(r => new ProductSummaryDto(
                r.Id,
                lang.Localize(r.NameEn, r.NameAr),
                r.ThumbnailUrl,
                r.Price,
                r.FinalPrice,
                r.ComparePrice,
                InStock: IsInStock(r.TrackInventory, r.StockQuantity),
                r.ToDiscount(DiscountSources.Section)))
            .ToList();

        return PagedResult<ProductSummaryDto>.Create(items, page, pageSize, total);
    }

    private static bool IsInStock(bool trackInventory, int stockQuantity)
        => !trackInventory || stockQuantity > 0;

    private static class DiscountSources
    {
        public const string Product = "Product";
        public const string Section = "Section";
    }

    // ════════════════════════════════════════════════════════════════════
    //  SQL FRAGMENTS
    //  Written once, reused by every query that needs discount logic.
    // ════════════════════════════════════════════════════════════════════

    private static class Sql
    {
        /// <summary>Discount is enabled and "now" falls inside its date window. Alias: d.</summary>
        public const string ActiveDiscountWindow = """
            d.is_active = true
            AND (d.start_date IS NULL OR d.start_date <= NOW())
            AND (d.end_date   IS NULL OR d.end_date   >= NOW())
            """;

        /// <summary>
        /// Picks the single best (lowest final price) active discount that applies to
        /// product `p`, either directly or through its section.
        /// Exposes: discount_id, discount_type, discount_value, discount_ends_at,
        /// discount_source, final_price. Requires alias: p (products).
        /// </summary>
        public const string ProductDiscountJoin = $"""
            LEFT JOIN LATERAL (
                SELECT
                    d.id         AS discount_id,
                    d.type::text AS discount_type,
                    d.value      AS discount_value,
                    d.end_date   AS discount_ends_at,
                    CASE
                        WHEN d.type = 'Percentage' THEN ROUND(p.price - p.price * d.value / 100, 2)
                        ELSE GREATEST(p.price - d.value, 0)
                    END AS final_price,
                    CASE
                        WHEN EXISTS (
                            SELECT 1 FROM discount_products dp
                            WHERE dp.discount_id = d.id AND dp.product_id = p.id
                        ) THEN '{DiscountSources.Product}'
                        ELSE '{DiscountSources.Section}'
                    END AS discount_source
                FROM discounts d
                WHERE d.store_id = p.store_id
                  AND {ActiveDiscountWindow}
                  AND (
                        EXISTS (SELECT 1 FROM discount_products dp
                                WHERE dp.discount_id = d.id AND dp.product_id = p.id)
                     OR EXISTS (SELECT 1 FROM discount_sections ds
                                WHERE ds.discount_id = d.id AND ds.section_id = p.section_id)
                  )
                ORDER BY final_price ASC, d.id
                LIMIT 1
            ) bd ON true
            """;

        /// <summary>
        /// Picks the best active discount attached to section `s`
        /// (percentage first, then highest value). Requires alias: s (store_sections).
        /// </summary>
        public const string SectionDiscountJoin = $"""
            LEFT JOIN LATERAL (
                SELECT
                    d.id         AS discount_id,
                    d.type::text AS discount_type,
                    d.value      AS discount_value,
                    d.end_date   AS discount_ends_at
                FROM discount_sections ds
                JOIN discounts d ON d.id = ds.discount_id
                WHERE ds.section_id = s.id
                  AND d.store_id = s.store_id
                  AND {ActiveDiscountWindow}
                ORDER BY (d.type = 'Percentage') DESC, d.value DESC, d.id
                LIMIT 1
            ) sd ON true
            """;

        /// <summary>Column list for product cards. Requires ProductDiscountJoin (bd) and alias p.</summary>
        public const string ProductSummaryColumns = """
            p.id,
            p.name_en,
            p.name_ar,
            p.price,
            COALESCE(bd.final_price, p.price) AS final_price,
            p.compare_price,
            p.track_inventory,
            p.stock_quantity,
            bd.discount_id,
            bd.discount_type,
            bd.discount_value,
            bd.discount_ends_at,
            bd.discount_source,
            (
                SELECT pi.image_url
                FROM product_images pi
                WHERE pi.product_id = p.id
                ORDER BY pi.display_order, pi.id
                LIMIT 1
            ) AS thumbnail_url
            """;
    }

    // ════════════════════════════════════════════════════════════════════
    //  ROW MODELS (Dapper)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>Shared discount columns + mapping to <see cref="DiscountInfoDto"/>.</summary>
    private abstract class DiscountRowBase
    {
        public Guid? DiscountId { get; init; }
        public string? DiscountType { get; init; }
        public decimal? DiscountValue { get; init; }
        public DateTime? DiscountEndsAt { get; init; }
        public string? DiscountSource { get; init; }

        public DiscountInfoDto? ToDiscount(string defaultSource)
            => DiscountId is null || DiscountType is null
                ? null
                : new DiscountInfoDto(
                    DiscountId.Value,
                    DiscountType,
                    DiscountValue ?? 0m,
                    DiscountEndsAt,
                    DiscountSource ?? defaultSource);
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
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public decimal RatingAvg { get; init; }
        public int ReviewCount { get; init; }
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

    private sealed class StoreCouponRow
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = default!;
        public short DiscountType { get; init; }
        public decimal DiscountValue { get; init; }
        public decimal? MaxDiscountAmount { get; init; }
        public decimal MinOrderAmount { get; init; }
        public DateTime StartsAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
    }

    private sealed class StoreSectionRow : DiscountRowBase
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? ImageUrl { get; init; }
    }

    private sealed class ProductSummaryRow : DiscountRowBase
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public decimal Price { get; init; }
        public decimal FinalPrice { get; init; }
        public decimal? ComparePrice { get; init; }
        public bool TrackInventory { get; init; }
        public int StockQuantity { get; init; }
        public string? ThumbnailUrl { get; init; }
    }

    private sealed class ProductDetailRow : DiscountRowBase
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public decimal Price { get; init; }
        public decimal FinalPrice { get; init; }
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
    }

    private sealed class OptionRow
    {
        public Guid Id { get; init; }
        public Guid OptionGroupId { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public decimal PriceAdjustment { get; init; }
        public bool IsDefault { get; init; }
    }
}