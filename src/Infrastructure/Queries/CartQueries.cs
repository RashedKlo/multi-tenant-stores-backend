using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Dapper;
using Infrastructure.Persistence;

public class CartQueries : ICartQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public CartQueries(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    // ───────────────────────── Public API ─────────────────────────

    public async Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(
        Guid? customerId,
        Guid? guestSessionId,
        Language lang,
        CancellationToken cancellationToken = default)
    {
        var rows = await QueryCartRowsAsync(
            customerId: customerId,
            guestSessionId: guestSessionId,
            storeId: null,
            includeInventory: false,
            cancellationToken);

        return rows.Select(r => MapToCartItemDto(r, lang)).ToList();
    }

    public async Task<CheckoutCartDto?> GetCartForCheckoutAsync(
        Guid customerId,
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var rows = await QueryCartRowsAsync(
            customerId: customerId,
            guestSessionId: null,
            storeId: storeId,
            includeInventory: true,
            cancellationToken);

        if (rows.Count == 0)
            return null;

        var items = rows.Select(MapToCheckoutItemDto).ToList();
        return new CheckoutCartDto(rows[0].CartId, rows[0].StoreId, items);
    }

    // ───────────────────────── Shared query ─────────────────────────

    private async Task<IReadOnlyList<CartRow>> QueryCartRowsAsync(
        Guid? customerId,
        Guid? guestSessionId,
        Guid? storeId,
        bool includeInventory,
        CancellationToken cancellationToken)
    {
        // Single SQL that can serve both callers.
        // Inventory / image columns are always selected; mapping decides what to use.
        const string sql = """
            SELECT
                c.id                                              AS CartId,
                c.store_id                                        AS StoreId,
                ci.id                                             AS CartItemId,
                ci.product_id                                     AS ProductId,
                p.name_en                                         AS NameEn,
                p.name_ar                                         AS NameAr,
                pi.image_url                                      AS ProductImage,          -- used by display path
                p.price                                           AS UnitPrice,
                ci.quantity                                       AS Quantity,
                ci.notes                                          AS Notes,
                p.track_inventory                                 AS TrackInventory,        -- used by checkout
                p.stock_quantity                                  AS StockQuantity,
                p.is_active                                       AS IsActive,
                p.deleted_at                                      AS DeletedAt,
                COALESCE(opts.selected_options, '[]')             AS SelectedOptionsJson,
                (p.price + COALESCE(opts.options_total, 0)) * ci.quantity AS ItemTotalPrice
            FROM carts c
            JOIN cart_items ci ON ci.cart_id = c.id
            JOIN products p    ON p.id = ci.product_id
            LEFT JOIN product_images pi ON pi.product_id = p.id AND pi.display_order = 1
            LEFT JOIN LATERAL (
                SELECT
                    json_agg(
                        json_build_object(
                            'option_id',          po.id,
                            'group_name_en',      pog.name_en,
                            'group_name_ar',      pog.name_ar,
                            'option_name_en',     po.name_en,
                            'option_name_ar',     po.name_ar,
                            'price_adjustment',   po.price_adjustment,
                            'is_active',          po.is_active,
                            'deleted_at',         po.deleted_at
                        ) ORDER BY po.display_order
                    ) AS selected_options,
                    SUM(po.price_adjustment) AS options_total
                FROM cart_item_options cio
                JOIN product_options po ON po.id = cio.option_id
                JOIN product_option_groups pog ON pog.id = po.option_group_id
                WHERE cio.cart_item_id = ci.id
            ) opts ON true
            WHERE (
                    (@CustomerId::uuid IS NOT NULL AND c.customer_id = @CustomerId)
                 OR (@GuestSessionId::uuid IS NOT NULL AND c.guest_session_id = @GuestSessionId)
                  )
              AND (@StoreId::uuid IS NULL OR c.store_id = @StoreId)
            ORDER BY ci.created_at
            """;

        await using var connection = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<CartRow>(
            new CommandDefinition(
                sql,
                new
                {
                    CustomerId = customerId,
                    GuestSessionId = guestSessionId,
                    StoreId = storeId
                },
                cancellationToken: cancellationToken));

        return rows.ToList();
    }

    // ───────────────────────── Mapping ─────────────────────────

    private static CartItemDto MapToCartItemDto(CartRow row, Language lang)
    {
        var raw = JsonSerializer.Deserialize<List<OptionRaw>>(row.SelectedOptionsJson, JsonOptions) ?? [];

        var options = raw.Select(o => new SelectedOptionDto(
            o.OptionId,
            lang.Localize(o.GroupNameEn, o.GroupNameAr),
            lang.Localize(o.OptionNameEn, o.OptionNameAr),
            o.PriceAdjustment
        )).ToList();

        return new CartItemDto(
            row.CartItemId,
            row.CartId,
            row.StoreId,
            row.ProductId,
            lang.Localize(row.NameEn, row.NameAr),
            row.ProductImage,
            row.UnitPrice,               // BasePrice
            row.Quantity,
            row.Notes,
            options,
            row.ItemTotalPrice);
    }

    private static CheckoutCartItemDto MapToCheckoutItemDto(CartRow row)
    {
        var raw = JsonSerializer.Deserialize<List<OptionRaw>>(row.SelectedOptionsJson, JsonOptions) ?? [];

        var options = raw.Select(o => new CheckoutOptionDto(
            o.OptionId,
            o.OptionNameEn,
            o.OptionNameAr,
            o.PriceAdjustment,
            o.IsActive,
            o.DeletedAt
        )).ToList();

        return new CheckoutCartItemDto(
            row.CartItemId,
            row.ProductId,
            row.NameEn,
            row.NameAr,
            row.UnitPrice,
            row.Quantity,
            row.Notes,
            row.TrackInventory,
            row.StockQuantity,
            row.IsActive,
            row.DeletedAt,
            options);
    }

    // ───────────────────────── Private rows ─────────────────────────

    private sealed class CartRow
    {
        public Guid CartId { get; init; }
        public Guid StoreId { get; init; }
        public Guid CartItemId { get; init; }
        public Guid ProductId { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? ProductImage { get; init; }
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? Notes { get; init; }
        public bool TrackInventory { get; init; }
        public int StockQuantity { get; init; }
        public bool IsActive { get; init; }
        public DateTime? DeletedAt { get; init; }
        public string SelectedOptionsJson { get; init; } = "[]";
        public decimal ItemTotalPrice { get; init; }
    }

    private sealed class OptionRaw
    {
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