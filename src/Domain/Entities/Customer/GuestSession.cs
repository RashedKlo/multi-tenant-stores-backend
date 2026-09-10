// Domain/Entities/Customer/GuestSession.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class GuestSession
{
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsExpired;


    private GuestSession() { }

    public static Result<GuestSession> Create(string tokenHash, DateTime expiresAt)
    {
        var session = new GuestSession
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        return session
            .SetTokenHash(tokenHash)
            .Bind(() => session.SetExpiresAt(expiresAt))
            .Bind(() => Result<GuestSession>.Success(session));
    }

    private Result SetTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            return Result.Failure(Error.Validation(
                "GuestSession.TokenHash.Required", "Token hash is required."));

        TokenHash = tokenHash.Trim();
        return Result.Success();
    }

    private Result SetExpiresAt(DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            return Result.Failure(Error.Validation(
                "GuestSession.ExpiresAt.Invalid", "Expiry must be in the future."));

        ExpiresAt = expiresAt;
        return Result.Success();
    }

    public Result Touch()
    {
        if (IsExpired)
            return Result.Failure(Error.Validation(
                "GuestSession.Expired", "Cannot touch an expired guest session."));

        LastSeenAt = DateTime.UtcNow;
        return Result.Success();
    }
}