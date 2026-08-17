using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Google.Apis.Auth;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Requires the Google.Apis.Auth NuGet package.
/// </summary>
public class GoogleTokenVerifier(
    IOptions<GoogleAuthSettings> options,
    ILogger<GoogleTokenVerifier> logger) : IGoogleTokenVerifier
{
    private readonly GoogleAuthSettings _settings = options.Value;

    public async Task<GoogleUserInfo> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validates signature, expiry, issuer, AND audience against our client id.
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_settings.ClientId]
                });

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.GivenName ?? string.Empty,
                payload.FamilyName ?? string.Empty);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed");
            throw new ForbiddenException("Invalid or expired Google ID token.");
        }
    }
}
