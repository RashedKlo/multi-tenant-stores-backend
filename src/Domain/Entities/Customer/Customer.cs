// Domain/Entities/Customer/Customer.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? GoogleId { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

 
    private Customer() { }

    // ---------- Factories ----------

    public static Result<Customer> CreateWithPassword(
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = false
        };

        return customer
            .SetFirstName(firstName)
            .Bind(() => customer.SetLastName(lastName))
            .Bind(() => customer.SetEmail(email))
            .Bind(() => customer.SetPasswordHash(passwordHash))
            .Bind(() => Result<Customer>.Success(customer));
    }

    public static Result<Customer> CreateWithGoogle(
        string firstName,
        string lastName,
        string email,
        string googleId)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = true // Google accounts are trusted
        };

        return customer
            .SetFirstName(firstName)
            .Bind(() => customer.SetLastName(lastName))
            .Bind(() => customer.SetEmail(email))
            .Bind(() => customer.LinkGoogleAccount(googleId))
            .Bind(() => Result<Customer>.Success(customer));
    }

    // ---------- Field-level setters ----------

    private Result SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(Error.Validation("Customer.FirstName.Required", "First name is required."));

        var trimmed = firstName.Trim();
        if (trimmed.Length > 100)
            return Result.Failure(Error.Validation("Customer.FirstName.TooLong", "First name cannot exceed 100 characters."));

        FirstName = trimmed;
        return Result.Success();
    }

    private Result SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(Error.Validation("Customer.LastName.Required", "Last name is required."));

        var trimmed = lastName.Trim();
        if (trimmed.Length > 100)
            return Result.Failure(Error.Validation("Customer.LastName.TooLong", "Last name cannot exceed 100 characters."));

        LastName = trimmed;
        return Result.Success();
    }

    private Result SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(Error.Validation("Customer.Email.Required", "Email is required."));

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 255)
            return Result.Failure(Error.Validation("Customer.Email.TooLong", "Email cannot exceed 255 characters."));

        // Simple format check — deeper validation can stay in FluentValidation
        if (!normalized.Contains('@') || !normalized.Contains('.'))
            return Result.Failure(Error.Validation("Customer.Email.Invalid", "Email format is invalid."));

        Email = normalized;
        return Result.Success();
    }

    private Result SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(Error.Validation("Customer.PasswordHash.Required", "Password hash is required."));

        PasswordHash = passwordHash;
        return Result.Success();
    }

    // ---------- Behavior ----------

    public Result UpdateProfile(string firstName, string lastName)
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Customer.Deleted", "Cannot update a deleted customer."));

        if (!IsActive)
            return Result.Failure(Error.Validation("Customer.Inactive", "Cannot update an inactive customer."));

        return SetFirstName(firstName)
            .Bind(() => SetLastName(lastName))
            .Bind(() =>
            {
                Touch();
                return Result.Success();
            });
    }

    /// <summary>
    /// Replaces the password hash. Caller (Application) is responsible for verifying the old password
    /// and hashing the new one before calling this method.
    /// </summary>
    public Result ChangePassword(string newPasswordHash)
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Customer.Deleted", "Cannot change password of a deleted customer."));

        if (!IsActive)
            return Result.Failure(Error.Validation("Customer.Inactive", "Cannot change password of an inactive customer."));

        // Google-only accounts may not have a password yet — still allow setting one
        return SetPasswordHash(newPasswordHash)
            .Bind(() =>
            {
                Touch();
                return Result.Success();
            });
    }

    public Result VerifyEmail()
    {
        if (IsEmailVerified)
            return Result.Success(); // idempotent

        IsEmailVerified = true;
        Touch();
        return Result.Success();
    }

    public Result LinkGoogleAccount(string googleId)
    {
        if (string.IsNullOrWhiteSpace(googleId))
            return Result.Failure(Error.Validation("Customer.GoogleId.Required", "GoogleId is required."));

        if (GoogleId is not null && GoogleId != googleId)
            return Result.Failure(Error.Conflict("Customer.GoogleId.AlreadyLinked", "A different Google account is already linked."));

        GoogleId = googleId.Trim();
        if (!IsEmailVerified)
            IsEmailVerified = true;

        Touch();
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Success();

        IsActive = true;
        Touch();
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Success();

        IsActive = false;
        Touch();
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Success();

        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        Touch();
        return Result.Success();
    }

    public Result Restore()
    {
        if (!IsDeleted)
            return Result.Success();

        DeletedAt = null;
        IsActive = true;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}