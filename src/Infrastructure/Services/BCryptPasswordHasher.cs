using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Requires the BCrypt.Net-Next NuGet package.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor 12 ≈ 250–300ms per hash on typical hardware —
    // high enough to slow brute-force, low enough that login/register stay responsive.
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Malformed / foreign-format hash → fail verification, don't crash login.
            return false;
        }
    }
}
