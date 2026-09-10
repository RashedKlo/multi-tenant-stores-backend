using Domain.Common;

namespace Domain.Entities;

public sealed class ModuleBanner
{
    public Guid Id { get; private set; }
    public Guid ModuleId { get; private set; }
    public string ImageUrl { get; private set; } = null!;
    public string? TitleEn { get; private set; }
    public string? TitleAr { get; private set; }
    public string? ActionUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private ModuleBanner()
    {
    }

}