using Domain.Common;

namespace Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    private Tenant()
    {
    }

    public static Result<Tenant> Create(string name, string email, string passwordHash)
    {
        var tenant = new Tenant();

        return tenant
            .SetName(name)
            .Bind(() => tenant.SetEmail(email))
            .Bind(() => tenant.SetPasswordHash(passwordHash))
            .Bind(() => tenant.Initialize())
            .Bind(() => Result<Tenant>.Success(tenant));
    }

    public Result Update(string name, string email, string passwordHash)
    {
        return SetName(name)
            .Bind(() => SetEmail(email))
            .Bind(() => SetPasswordHash(passwordHash))
            .Bind(() =>
            {
                UpdatedAt = DateTime.UtcNow;
                return Result.Success();
            });
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        Touch();
    }

    public void Restore()
    {
        DeletedAt = null;
        Touch();
    }

    private Result Initialize()
    {
        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        return Result.Success();
    }

    private Result SetName(string name)
    {
        var errors = new List<Error>();
        name = DomainValidation.NormalizeRequiredString(name, errors, "Name");

        if (errors.Count > 0)
            return Result.Failure(errors);

        Name = name;
        return Result.Success();
    }

    private Result SetEmail(string email)
    {
        var errors = new List<Error>();
        email = DomainValidation.NormalizeRequiredEmail(email, errors);

        if (errors.Count > 0)
            return Result.Failure(errors);

        Email = email;
        return Result.Success();
    }

    private Result SetPasswordHash(string passwordHash)
    {
        var errors = new List<Error>();
        passwordHash = DomainValidation.NormalizeRequiredHash(passwordHash, errors);

        if (errors.Count > 0)
            return Result.Failure(errors);

        PasswordHash = passwordHash;
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}