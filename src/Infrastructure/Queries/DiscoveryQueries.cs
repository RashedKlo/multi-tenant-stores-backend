using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Dapper;
using Infrastructure.Persistence;

namespace Infrastructure.Queries;

public sealed class DiscoveryQueries : IDiscoveryQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DiscoveryQueries(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<HomeBannerDto>> GetHomeBannersAsync(
        Language lang,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, image_url, title_en, title_ar, subtitle_en, subtitle_ar, action_url
            FROM home_banners
            WHERE is_active = true
            ORDER BY display_order, id
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();
        var rows = await conn.QueryAsync<HomeBannerRow>(
            new CommandDefinition(sql, cancellationToken: ct));

        return rows.Select(r => new HomeBannerDto(
            r.Id,
            r.ImageUrl,
            lang.LocalizeNullable(r.TitleEn, r.TitleAr),
            lang.LocalizeNullable(r.SubtitleEn, r.SubtitleAr),
            r.ActionUrl)).ToList();
    }

    public async Task<IReadOnlyList<ModuleDto>> GetModulesAsync(
        Language lang,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name_en, name_ar, icon_url
            FROM modules
            WHERE is_active = true
            ORDER BY display_order, id
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();
        var rows = await conn.QueryAsync<ModuleRow>(
            new CommandDefinition(sql, cancellationToken: ct));

        return rows.Select(r => new ModuleDto(
            r.Id,
            lang.Localize(r.NameEn, r.NameAr),
            r.IconUrl)).ToList();
    }

    public async Task<ModuleDetailDto?> GetModuleDetailAsync(
        Guid moduleId,
        Language lang,
        CancellationToken ct = default)
    {
        const string moduleSql = """
            SELECT id, name_en, name_ar, icon_url
            FROM modules
            WHERE id = @ModuleId AND is_active = true
            """;

        const string bannersSql = """
            SELECT id, image_url, title_en, title_ar, action_url
            FROM module_banners
            WHERE module_id = @ModuleId AND is_active = true
            ORDER BY display_order, id
            """;

        const string categoriesSql = """
            SELECT id, name_en, name_ar, image_url
            FROM categories
            WHERE module_id = @ModuleId AND is_active = true
            ORDER BY display_order, id
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var module = await conn.QuerySingleOrDefaultAsync<ModuleRow>(
            new CommandDefinition(moduleSql, new { ModuleId = moduleId }, cancellationToken: ct));

        if (module is null) return null;

        var banners = (await conn.QueryAsync<ModuleBannerRow>(
            new CommandDefinition(bannersSql, new { ModuleId = moduleId }, cancellationToken: ct))).ToList();

        var categories = (await conn.QueryAsync<CategoryRow>(
            new CommandDefinition(categoriesSql, new { ModuleId = moduleId }, cancellationToken: ct))).ToList();

        return new ModuleDetailDto(
            module.Id,
            lang.Localize(module.NameEn, module.NameAr),
            module.IconUrl,
            banners.Select(b => new ModuleBannerDto(
                b.Id,
                b.ImageUrl,
                lang.LocalizeNullable(b.TitleEn, b.TitleAr),
                b.ActionUrl)).ToList(),
            categories.Select(c => new CategoryDto(
                c.Id,
                lang.Localize(c.NameEn, c.NameAr),
                c.ImageUrl)).ToList());
    }

    public async Task<PagedResult<StoreSummaryDto>> GetStoresByModuleAsync(
        Guid moduleId,
        Guid? categoryId,
        string? search,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default)
    {
        var filters = new List<string>
        {
            "s.module_id = @ModuleId",
            "s.is_active = true",
            "s.deleted_at IS NULL"
        };

        if (categoryId.HasValue)
            filters.Add("""
                EXISTS (
                    SELECT 1 FROM store_categories sc
                    WHERE sc.store_id = s.id AND sc.category_id = @CategoryId
                )
                """);

        if (!string.IsNullOrWhiteSpace(search))
            filters.Add("(s.name_en ILIKE @Search OR s.name_ar ILIKE @Search)");

        var where = string.Join(" AND ", filters);

        var countSql = $"""SELECT COUNT(*) FROM stores s WHERE {where}""";

        var dataSql = $"""
            SELECT s.id, s.name_en, s.name_ar, s.logo_url, s.rating
            FROM stores s
            WHERE {where}
            ORDER BY s.rating DESC, s.name_en
            OFFSET @Offset LIMIT @PageSize
            """;

        await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection();

        var param = new
        {
            ModuleId = moduleId,
            CategoryId = categoryId,
            Search = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%",
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: ct));

        var rows = await conn.QueryAsync<StoreSummaryRow>(
            new CommandDefinition(dataSql, param, cancellationToken: ct));

        var items = rows.Select(r => new StoreSummaryDto(
            r.Id,
            lang.Localize(r.NameEn, r.NameAr),
            r.LogoUrl,
            r.Rating)).ToList();

        return PagedResult<StoreSummaryDto>.Create(items, page, pageSize, total);
    }

    private sealed class HomeBannerRow
    {
        public Guid Id { get; init; }
        public string ImageUrl { get; init; } = default!;
        public string? TitleEn { get; init; }
        public string? TitleAr { get; init; }
        public string? SubtitleEn { get; init; }
        public string? SubtitleAr { get; init; }
        public string? ActionUrl { get; init; }
    }

    private sealed class ModuleRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? IconUrl { get; init; }
    }

    private sealed class ModuleBannerRow
    {
        public Guid Id { get; init; }
        public string ImageUrl { get; init; } = default!;
        public string? TitleEn { get; init; }
        public string? TitleAr { get; init; }
        public string? ActionUrl { get; init; }
    }

    private sealed class CategoryRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? ImageUrl { get; init; }
    }

    private sealed class StoreSummaryRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public string? LogoUrl { get; init; }
        public decimal Rating { get; init; }
    }
}
