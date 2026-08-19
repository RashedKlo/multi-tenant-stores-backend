using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Models;
using Dapper;
using Infrastructure.Persistence;

namespace Infrastructure.Queries;

public class CartQueries : ICartQueries
{
    private readonly IDbConnectionFactory _connectionFactory;
    private static readonly JsonSerializerOptions JsonOptions = new();

    public CartQueries(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(Guid cartId)
    {
        const string sql = """
            SELECT
                ci.id                                             AS CartItemId,
                ci.cart_id                                         AS CartId,
                ci.product_id                                      AS ProductId,
                p.name_en                                          AS ProductNameEn,
                p.name_ar                                          AS ProductNameAr,
                p.price                                            AS BasePrice,
                ci.quantity                                        AS Quantity,
                ci.notes                                           AS Notes,
                COALESCE(opts.selected_options, '[]')              AS SelectedOptionsJson,
                (p.price + COALESCE(opts.options_total, 0)) * ci.quantity AS ItemTotalPrice
            FROM cart_items ci
            JOIN products p ON p.id = ci.product_id
            LEFT JOIN LATERAL (
                SELECT
                    json_agg(json_build_object(
                        'option_id', po.id,
                        'group_name_en', pog.name_en,
                        'group_name_ar', pog.name_ar,
                        'option_name_en', po.name_en,
                        'option_name_ar', po.name_ar,
                        'price_adjustment', po.price_adjustment
                    )) AS selected_options,
                    SUM(po.price_adjustment) AS options_total
                FROM cart_item_options cio
                JOIN product_options po ON po.id = cio.option_id
                JOIN product_option_groups pog ON pog.id = po.option_group_id
                WHERE cio.cart_item_id = ci.id
            ) opts ON true
            WHERE ci.cart_id = @CartId
            ORDER BY ci.created_at
            """;

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<CartItemRow>(sql, new { CartId = cartId });

        return rows.Select(MapToDto).ToList();
    }

    private static CartItemDto MapToDto(CartItemRow row)
    {
        var options = JsonSerializer.Deserialize<List<SelectedOptionDto>>(
            row.SelectedOptionsJson, JsonOptions) ?? [];

        return new CartItemDto(
            row.CartItemId,
            row.CartId,
            row.ProductId,
            row.ProductNameEn,
            row.ProductNameAr,
            row.BasePrice,
            row.Quantity,
            row.Notes,
            options,
            row.ItemTotalPrice);
    }

    // Flat row shape Dapper maps directly from the query — SelectedOptions
    // arrives as raw JSON text here and gets deserialized in MapToDto.
    private sealed class CartItemRow
    {
        public Guid CartItemId { get; init; }
        public Guid CartId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductNameEn { get; init; } = default!;
        public string ProductNameAr { get; init; } = default!;
        public decimal BasePrice { get; init; }
        public int Quantity { get; init; }
        public string? Notes { get; init; }
        public string SelectedOptionsJson { get; init; } = "[]";
        public decimal ItemTotalPrice { get; init; }
    }
}