using Domain.Common;

namespace Domain.Entities;

public sealed class HomeBanner
{
    public Guid Id { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string? TitleEn { get; private set; }
    public string? TitleAr { get; private set; }
    public string? SubtitleEn { get; private set; }
    public string? SubtitleAr { get; private set; }
    public string? ActionUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private HomeBanner()
    {
    }

    public static Result<HomeBanner> Create(
        string imageUrl,
        string? titleEn = null,
        string? titleAr = null,
        string? subtitleEn = null,
        string? subtitleAr = null,
        string? actionUrl = null,
        int displayOrder = 0,
        bool isActive = true)
    {
        var banner = new HomeBanner();

        return banner
            .SetImageUrl(imageUrl)
            .Bind(() => banner.SetDisplayOrder(displayOrder))
            .Bind(() => banner.SetTitleEn(titleEn))
            .Bind(() => banner.SetTitleAr(titleAr))
            .Bind(() => banner.SetSubtitleEn(subtitleEn))
            .Bind(() => banner.SetSubtitleAr(subtitleAr))
            .Bind(() => banner.SetActionUrl(actionUrl))
            .Bind(() => banner.SetIsActive(isActive))
            .Bind(() => banner.Initialize())
            .Bind(() => Result<HomeBanner>.Success(banner));
    }

    public Result Update(
        string imageUrl,
        string? titleEn = null,
        string? titleAr = null,
        string? subtitleEn = null,
        string? subtitleAr = null,
        string? actionUrl = null,
        int displayOrder = 0)
    {
        return SetImageUrl(imageUrl)
            .Bind(() => SetDisplayOrder(displayOrder))
            .Bind(() => SetTitleEn(titleEn))
            .Bind(() => SetTitleAr(titleAr))
            .Bind(() => SetSubtitleEn(subtitleEn))
            .Bind(() => SetSubtitleAr(subtitleAr))
            .Bind(() => SetActionUrl(actionUrl));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private Result Initialize()
    {
        Id = Guid.NewGuid();
        return Result.Success();
    }

    private Result SetImageUrl(string imageUrl)
    {
        var errors = new List<Error>();
        imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");

        if (errors.Count > 0)
            return Result.Failure(errors);

        ImageUrl = imageUrl;
        return Result.Success();
    }

    private Result SetDisplayOrder(int displayOrder)
    {
        var errors = new List<Error>();
        DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

        if (errors.Count > 0)
            return Result.Failure(errors);

        DisplayOrder = displayOrder;
        return Result.Success();
    }

    private Result SetTitleEn(string? titleEn)
    {
        TitleEn = DomainValidation.NormalizeOptional(titleEn);
        return Result.Success();
    }

    private Result SetTitleAr(string? titleAr)
    {
        TitleAr = DomainValidation.NormalizeOptional(titleAr);
        return Result.Success();
    }

    private Result SetSubtitleEn(string? subtitleEn)
    {
        SubtitleEn = DomainValidation.NormalizeOptional(subtitleEn);
        return Result.Success();
    }

    private Result SetSubtitleAr(string? subtitleAr)
    {
        SubtitleAr = DomainValidation.NormalizeOptional(subtitleAr);
        return Result.Success();
    }

    private Result SetActionUrl(string? actionUrl)
    {
        ActionUrl = DomainValidation.NormalizeOptional(actionUrl);
        return Result.Success();
    }

    private Result SetIsActive(bool isActive)
    {
        IsActive = isActive;
        return Result.Success();
    }
}