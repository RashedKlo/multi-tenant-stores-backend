namespace Application.Common.Interfaces;

public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);

public interface IJwtTokenService
{
    /// <summary>
    /// Issues a short-lived JWT access token + opaque refresh token (raw).
    /// Caller must hash the refresh token before persisting.
    /// </summary>
    TokenPair GenerateTokenPair(Guid customerId, string email);

    /// <summary>
    /// 256-bit URL-safe opaque token — used for refresh tokens and guest sessions.
    /// </summary>
    string GenerateOpaqueToken();

    /// <summary>
    /// SHA-256 hex hash of a raw opaque token. Never store raw tokens.
    /// </summary>
    string HashToken(string rawToken);

    /// <summary>
    /// Validates an access JWT and returns the customer id from <c>sub</c>,
    /// or null when the token is missing/invalid/expired.
    /// </summary>
    Guid? ValidateAccessTokenAndGetCustomerId(string accessToken);
}
