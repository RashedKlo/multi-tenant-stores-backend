namespace Application.Common.Interfaces;

public record GoogleUserInfo(
    string GoogleId,
    string Email,
    string FirstName,
    string LastName);

public interface IGoogleTokenVerifier
{
    /// <summary>
    /// Verifies the Google ID token (signature, expiry, audience) and returns profile claims.
    /// Throws on invalid / expired / wrong-audience tokens.
    /// </summary>
    Task<GoogleUserInfo> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}
