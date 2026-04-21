using Domain.Entities;

public class Store
{

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; }
    public ICollection<Department> Departments { get; private set; } = new List<Department>();

    private Store() { }

    public static Store Create(Guid tenantId, string name, string? description, string? logoUrl, string? bannerUrl)
    {
        return new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            LogoUrl = logoUrl,
            BannerUrl = bannerUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, string? logoUrl, string? bannerUrl, bool isActive)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        BannerUrl = bannerUrl;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}