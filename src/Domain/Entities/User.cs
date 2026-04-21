namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? TenantId { get; private set; }   // null for SuperAdmin & Customer
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Tenant? Tenant { get; private set; }

    // EF Core needs this
    private User() { }

    public static User Create(string firstName, string lastName, string email, string passwordHash, UserRole role, Guid? tenantId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string firstName, string lastName, string email, UserRole role, Guid? tenantId, bool isActive)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role;
        TenantId = tenantId;
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

public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    Manager = 3,
    Employee = 4,
    Customer = 5
}
