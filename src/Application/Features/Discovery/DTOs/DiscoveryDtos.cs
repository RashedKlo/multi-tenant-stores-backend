using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Discovery.DTOs;

public record HomeBannerDto(
    Guid Id,
    string ImageUrl,
    string? Title,
    string? Subtitle,
    string? ActionUrl)
{
    public static HomeBannerDto FromEntity(Domain.Entities.HomeBanner b, Language lang) => new(
        b.Id, b.ImageUrl,
        lang.LocalizeNullable(b.TitleEn, b.TitleAr),
        lang.LocalizeNullable(b.SubtitleEn, b.SubtitleAr),
        b.ActionUrl);
}

public record ModuleDto(Guid Id, string Name, string? IconUrl)
{
    public static ModuleDto FromEntity(Domain.Entities.Module m, Language lang) =>
        new(m.Id, lang.Localize(m.NameEn, m.NameAr), m.IconUrl);
}

public record CategoryDto(Guid Id, string Name, string? ImageUrl)
{
    public static CategoryDto FromEntity(Domain.Entities.Category c, Language lang) =>
        new(c.Id, lang.Localize(c.NameEn, c.NameAr), c.ImageUrl);
}

public record ModuleBannerDto(Guid Id, string ImageUrl, string? Title, string? ActionUrl)
{
    public static ModuleBannerDto FromEntity(Domain.Entities.ModuleBanner b, Language lang) =>
        new(b.Id, b.ImageUrl, lang.LocalizeNullable(b.TitleEn, b.TitleAr), b.ActionUrl);
}

/// <summary>
/// Composition of Module + ModuleBanners + Categories.
/// Built in the handler — not a single repository call.
/// </summary>
public record ModuleDetailDto(
    Guid Id,
    string Name,
    string? IconUrl,
    List<ModuleBannerDto> Banners,
    List<CategoryDto> Categories)
{
    public static ModuleDetailDto FromEntity(
        Domain.Entities.Module m,
        List<Domain.Entities.ModuleBanner> banners,
        List<Domain.Entities.Category> categories,
        Language lang) =>
        new(m.Id, lang.Localize(m.NameEn, m.NameAr), m.IconUrl,
            banners.Select(b => ModuleBannerDto.FromEntity(b, lang)).ToList(),
            categories.Select(c => CategoryDto.FromEntity(c, lang)).ToList());
}

/// <summary>
/// Thin store card for browse grids. Full detail lives in Catalog module.
/// </summary>
public record StoreSummaryDto(Guid Id, string Name, string? LogoUrl, decimal Rating)
{
    public static StoreSummaryDto FromEntity(Domain.Entities.Store store, Language lang) =>
        new(store.Id, lang.Localize(store.NameEn, store.NameAr), store.LogoUrl, store.Rating);

    public static IReadOnlyList<StoreSummaryDto> FromEntities(
        IEnumerable<Domain.Entities.Store> stores, Language lang) =>
        stores.Select(s => FromEntity(s, lang)).ToList();
}