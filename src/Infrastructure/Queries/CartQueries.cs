using System.Data.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Dapper;
using Infrastructure.Persistence;

namespace Infrastructure.Queries;

public sealed class CartQueries : ICartQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CartQueries(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    // ════════════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(
        Guid? customerId,
        Guid? guestSessionId,
        Language lang,
        CancellationToken ct = default)
    {
        var (items, options) = await LoadCartAsync(customerId, guestSessionId, storeId: null, ct);

        return items
            .Select(i => ToCartItemDto(i, options[i.CartItemId], lang))
            .ToList();
    }

    public async Task<CheckoutCartDto?> GetCartForCheckoutAsync(
        Guid customerId,
        Guid storeId,
        CancellationToken ct = default)
    {
        var (items, options) = await LoadCartAsync(customerId, guestSessionId: null, storeId, ct);

        if (items.Count == 0) return null;

        return new CheckoutCartDto(
            items[0].CartId,
            items[0].StoreId,
            items.Select(i => ToCheckoutItemDto(i, options[i.CartItemId])).ToList());
    }

    // ════════════════════════════════════════════════════════════════════
    //  DATA ACCESS
    //  Two statements, one round-trip:
    //    1) cart lines  (+ product, thumbnail, validated discount)
    //    2) selected options for those lines
    //  Options are a separate result set (not json_agg) so Dapper maps them
    //  straight into typed rows: no JSON build, no JSON parse.
    // ════════════════════════════════════════════════════════════════════

    private async Task<(IReadOnlyList<CartItemRow> Items, ILookup<Guid, CartOptionRow> Options)> LoadCartAsync(
        Guid? customerId,
        Guid? guestSessionId,
        Guid? storeId,
        CancellationToken ct)
    {
        // Build the owner filter from the inputs instead of
        // "(@A IS NOT NULL AND ...) OR (@B IS NOT NULL AND ...)",
        // which can't use the partial unique indexes on carts.
        string ownerFilter;
        if (customerId is not null) ownerFilter = "c.customer_id = @CustomerId";
        else if (guestSessionId is not null) ownerFilter = "c.guest_session_id = @GuestSessionId";
        else return ([], Enumerable.Empty<CartOptionRow>().ToLookup(o => o.CartItemId));

        var scope = storeId is null
            ? ownerFilter
            : $"{ownerFilter} AND c.store_id = @StoreId";

        var sql = $"""
            SELECT
                c.id                AS cart_id,
                c.store_id,
                ci.id               AS cart_item_id,
                ci.product_id,
                ci.quantity,
                ci.notes,
                p.name_en,
                p.name_ar,
                p.price             AS unit_price,
                p.track_inventory,
                p.stock_quantity,
                p.is_active,
                p.deleted_at,
                img.image_url       AS product_image,
                d.id                AS discount_id,
                d.type::text        AS discount_type,
                d.value             AS discount_value,
                d.end_date          AS discount_ends_at,
                CASE
                    WHEN d.id IS NULL          THEN p.price
                    WHEN d.type = 'Percentage' THEN ROUND(p.price - p.price * d.value / 100, 2)
                    ELSE GREATEST(p.price - d.value, 0)
                END                 AS final_unit_price
            FROM carts c
            JOIN cart_items ci ON ci.cart_id = c.id
            JOIN products p    ON p.id = ci.product_id
            LEFT JOIN LATERAL (
                SELECT pi.image_url
                FROM product_images pi
                WHERE pi.product_id = p.id
                ORDER BY pi.display_order, pi.id
                LIMIT 1
            ) img ON true
            LEFT JOIN discounts d
                   ON d.id = ci.discount_id
                  AND {Sql.ValidDiscountForCartItem}
            WHERE {scope}
            ORDER BY ci.created_at, ci.id;

            SELECT
                cio.cart_item_id,
                po.id               AS option_id,
                pog.name_en         AS group_name_en,
                pog.name_ar         AS group_name_ar,
                po.name_en          AS option_name_en,
                po.name_ar          AS option_name_ar,
                po.price_adjustment,
                po.is_active,
                po.deleted_at
            FROM carts c
            JOIN cart_items ci             ON ci.cart_id = c.id
            JOIN cart_item_options cio     ON cio.cart_item_id = ci.id
            JOIN product_options po        ON po.id = cio.option_id
            JOIN product_option_groups pog ON pog.id = po.option_group_id
            WHERE {scope}
            ORDER BY po.display_order, po.id;
            """;

        await using var conn = (DbConnection)_connectionFactory.CreateConnection();

        await using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { CustomerId = customerId, GuestSessionId = guestSessionId, StoreId = storeId },
                cancellationToken: ct));

        var items = (await multi.ReadAsync<CartItemRow>()).ToList();
        var options = (await multi.ReadAsync<CartOptionRow>()).ToLookup(o => o.CartItemId);

        return (items, options);
    }

    // ════════════════════════════════════════════════════════════════════
    //  MAPPING
    // ════════════════════════════════════════════════════════════════════

    private static CartItemDto ToCartItemDto(
        CartItemRow item,
        IEnumerable<CartOptionRow> options,
        Language lang)
    {
        var selected = options
            .Select(o => new SelectedOptionDto(
                o.OptionId,
                lang.Localize(o.GroupNameEn, o.GroupNameAr),
                lang.Localize(o.OptionNameEn, o.OptionNameAr),
                o.PriceAdjustment))
            .ToList();

        var optionsTotal = selected.Sum(o => o.PriceAdjustment);

        return new CartItemDto(
            item.CartItemId,
            item.CartId,
            item.StoreId,
            item.ProductId,
            lang.Localize(item.NameEn, item.NameAr),
            item.ProductImage,
            item.UnitPrice,
            item.FinalUnitPrice,
            item.Quantity,
            item.Notes,
            item.ToDiscount(),
            selected,
            (item.FinalUnitPrice + optionsTotal) * item.Quantity);
    }

    private static CheckoutCartItemDto ToCheckoutItemDto(
        CartItemRow item,
        IEnumerable<CartOptionRow> options)
        => new(
            item.CartItemId,
            item.ProductId,
            item.NameEn,
            item.NameAr,
            item.UnitPrice,
            item.FinalUnitPrice,
            item.ToDiscount(),
            item.Quantity,
            item.Notes,
            item.TrackInventory,
            item.StockQuantity,
            item.IsActive,
            item.DeletedAt,
            options
                .Select(o => new CheckoutOptionDto(
                    o.OptionId,
                    o.OptionNameEn,
                    o.OptionNameAr,
                    o.PriceAdjustment,
                    o.IsActive,
                    o.DeletedAt))
                .ToList());

    // ════════════════════════════════════════════════════════════════════
    //  SQL FRAGMENTS
    // ════════════════════════════════════════════════════════════════════

    private static class Sql
    {
        /// <summary>
        /// Join condition for ci.discount_id. The stored id is only honoured while the
        /// discount is still valid: same store as the cart, enabled, inside its date
        /// window, and still attached to the product (directly or via its section).
        /// Otherwise the join yields NULL and the line is priced at full price.
        /// Requires aliases: d, c, ci, p.
        /// </summary>
        public const string ValidDiscountForCartItem = """
            d.store_id = c.store_id
            AND d.is_active = true
            AND (d.start_date IS NULL OR d.start_date <= NOW())
            AND (d.end_date   IS NULL OR d.end_date   >= NOW())
            AND (
                  EXISTS (SELECT 1 FROM discount_products dp
                          WHERE dp.discount_id = d.id AND dp.product_id = p.id)
               OR EXISTS (SELECT 1 FROM discount_sections ds
                          WHERE ds.discount_id = d.id AND ds.section_id = p.section_id)
            )
            """;
    }

    // ════════════════════════════════════════════════════════════════════
    //  ROW MODELS (Dapper)
    // ════════════════════════════════════════════════════════════════════

    private sealed class CartItemRow
    {
        public Guid CartId { get; init; }
        public Guid StoreId { get; init; }
        public Guid CartItemId { get; init; }
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public string? Notes { get; init; }

        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public decimal UnitPrice { get; init; }
        public decimal FinalUnitPrice { get; init; }
        public string? ProductImage { get; init; }

        public bool TrackInventory { get; init; }
        public int StockQuantity { get; init; }
        public bool IsActive { get; init; }
        public DateTime? DeletedAt { get; init; }

        public Guid? DiscountId { get; init; }
        public string? DiscountType { get; init; }
        public decimal? DiscountValue { get; init; }
        public DateTime? DiscountEndsAt { get; init; }

        public CartDiscountDto? ToDiscount()
            => DiscountId is null || DiscountType is null
                ? null
                : new CartDiscountDto(DiscountId.Value, DiscountType, DiscountValue ?? 0m, DiscountEndsAt);
    }

    private sealed class CartOptionRow
    {
        public Guid CartItemId { get; init; }
        public Guid OptionId { get; init; }
        public string GroupNameEn { get; init; } = default!;
        public string GroupNameAr { get; init; } = default!;
        public string OptionNameEn { get; init; } = default!;
        public string OptionNameAr { get; init; } = default!;
        public decimal PriceAdjustment { get; init; }
        public bool IsActive { get; init; }
        public DateTime? DeletedAt { get; init; }
    }
}