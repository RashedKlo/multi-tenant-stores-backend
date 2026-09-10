using Domain.Common;

namespace Domain.Entities;

public sealed class Store
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ModuleId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string? DescriptionEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? AddressEn { get; private set; }
    public string? AddressAr { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public decimal Rating { get; private set; }
    public string? Metadata { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    private Store()
    {
    }

}