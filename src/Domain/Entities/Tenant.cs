

namespace Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    // Navigation
    public ICollection<Store> Stores { get; private set; } = new List<Store>();

    // EF Core needs this
    private Tenant() { }

    public static Tenant Create(string name, string email, string passwordHash)
    {

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        return tenant;
    }

    public void Update(string name, string email, string passwordHash, bool isActive)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

}