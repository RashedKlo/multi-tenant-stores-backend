// Domain/Entities/Customer/RefreshToken.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;


    private RefreshToken() { }

    public static Result<RefreshToken> Create(
        Guid customerId,
        string tokenHash,
        DateTime expiresAt)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        return token
            .SetCustomerId(customerId)
            .Bind(() => token.SetTokenHash(tokenHash))
            .Bind(() => token.SetExpiresAt(expiresAt))
            .Bind(() => Result<RefreshToken>.Success(token));
    }

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation(
                "RefreshToken.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            return Result.Failure(Error.Validation(
                "RefreshToken.TokenHash.Required", "Token hash is required."));

        TokenHash = tokenHash.Trim();
        return Result.Success();
    }

    private Result SetExpiresAt(DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            return Result.Failure(Error.Validation(
                "RefreshToken.ExpiresAt.Invalid", "Expiry must be in the future."));

        ExpiresAt = expiresAt;
        return Result.Success();
    }

    public Result MarkUsed()
    {
        if (!IsActive)
            return Result.Failure(Error.Validation(
                "RefreshToken.Inactive", "Cannot use an inactive refresh token."));

        LastUsedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Revoke()
    {
        if (IsRevoked)
            return Result.Success(); // idempotent

        RevokedAt = DateTime.UtcNow;
        return Result.Success();
    }
}