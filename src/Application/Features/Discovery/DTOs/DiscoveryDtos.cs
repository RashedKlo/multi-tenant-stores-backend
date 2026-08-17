namespace Application.Discovery.DTOs;

public record HomeBannerDto(
    Guid Id,
    string ImageUrl,
    string? TitleEn,
    string? TitleAr,
    string? SubtitleEn,
    string? SubtitleAr,
    string? ActionUrl)
{
    public static HomeBannerDto FromEntity(Domain.Entities.HomeBanner b) => new(
        b.Id, b.ImageUrl, b.TitleEn, b.TitleAr, b.SubtitleEn, b.SubtitleAr, b.ActionUrl);
}

public record ModuleDto(Guid Id, string NameEn, string NameAr, string? IconUrl)
{
    public static ModuleDto FromEntity(Domain.Entities.Module m) =>
        new(m.Id, m.NameEn, m.NameAr, m.IconUrl);
}

public record CategoryDto(Guid Id, string NameEn, string NameAr, string? ImageUrl)
{
    public static CategoryDto FromEntity(Domain.Entities.Category c) =>
        new(c.Id, c.NameEn, c.NameAr, c.ImageUrl);
}

public record ModuleBannerDto(Guid Id, string ImageUrl, string? TitleEn, string? TitleAr, string? ActionUrl)
{
    public static ModuleBannerDto FromEntity(Domain.Entities.ModuleBanner b) =>
        new(b.Id, b.ImageUrl, b.TitleEn, b.TitleAr, b.ActionUrl);
}

/// <summary>
/// Composition of Module + ModuleBanners + Categories.
/// Built in the handler — not a single repository call.
/// </summary>
public record ModuleDetailDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? IconUrl,
    List<ModuleBannerDto> Banners,
    List<CategoryDto> Categories)
{
    public static ModuleDetailDto FromEntity(Domain.Entities.Module m) =>
        new(m.Id, m.NameEn, m.NameAr, m.IconUrl, new List<ModuleBannerDto>(), new List<CategoryDto>());
}



/// <summary>
/// Thin store card for browse grids.
/// Full detail lives in Catalog module.
/// </summary>
public record StoreSummaryDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? LogoUrl,
    decimal Rating)
{
    public static StoreSummaryDto FromEntity(
        Domain.Entities.Store store) =>
        new(
            store.Id,
            store.NameEn,
            store.NameAr,
            store.LogoUrl,
            store.Rating);

    public static IReadOnlyList<StoreSummaryDto> FromEntities(
        IEnumerable<Domain.Entities.Store> stores) =>
        stores
            .Select(store =>
                FromEntity(
                    store))
            .ToList();
}