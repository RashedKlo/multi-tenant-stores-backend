namespace Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;

    /// <summary>
    /// HMAC signing key — at least 32 characters.
    /// Load from user-secrets / environment / Key Vault — never commit real values.
    /// </summary>
    public string SigningKey { get; set; } = default!;

    public int AccessTokenMinutes { get; set; } = 15;
}
